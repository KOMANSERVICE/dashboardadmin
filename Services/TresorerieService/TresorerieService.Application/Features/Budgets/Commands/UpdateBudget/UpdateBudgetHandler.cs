using TresorerieService.Application.Features.Budgets.DTOs;

namespace TresorerieService.Application.Features.Budgets.Commands.UpdateBudget;

public class UpdateBudgetHandler(
        IGenericRepository<Budget> budgetRepository,
        IGenericRepository<Category> categoryRepository,
        IGenericRepository<BudgetCategory> budgetCategoryRepository,
        IGenericRepository<CashFlow> cashFlowRepository,
        DbContext dbContext,
        IUnitOfWork unitOfWork
    )
    : ICommandHandler<UpdateBudgetCommand, UpdateBudgetResult>
{
    public async Task<UpdateBudgetResult> Handle(
        UpdateBudgetCommand command,
        CancellationToken cancellationToken = default)
    {
        // Récupérer le budget existant
        var budget = await budgetRepository.GetByIdAsync(command.Id, cancellationToken);

        if (budget == null)
        {
            throw new NotFoundException($"Budget non trouvé");
        }

        // Vérifier que le budget appartient à la bonne application et boutique
        if (budget.ApplicationId != command.ApplicationId || budget.BoutiqueId != command.BoutiqueId)
        {
            throw new NotFoundException($"Budget non trouvé");
        }

        string? warning = null;

        // Vérifier si on tente de modifier startDate et s'il y a des dépenses
        if (command.StartDate.HasValue && command.StartDate.Value != budget.StartDate)
        {
            var hasExpenses = await HasExpensesAsync(budget.Id, command.ApplicationId, command.BoutiqueId, cancellationToken);
            if (hasExpenses)
            {
                throw new BadRequestException("Impossible de modifier la date de début car des dépenses sont déjà enregistrées");
            }
            budget.StartDate = command.StartDate.Value;
        }

        // Mettre à jour le nom si fourni
        if (!string.IsNullOrWhiteSpace(command.Name) && command.Name != budget.Name)
        {
            // Vérifier l'unicité du nouveau nom
            var existingBudgets = await budgetRepository.GetByConditionAsync(
                b => b.ApplicationId == command.ApplicationId
                     && b.BoutiqueId == command.BoutiqueId
                     && b.Name == command.Name
                     && b.Id != command.Id
                     && b.IsActive,
                cancellationToken);

            if (existingBudgets.Any())
            {
                throw new BadRequestException($"Un budget avec le nom '{command.Name}' existe déjà pour cette boutique");
            }

            budget.Name = command.Name;
        }

        // Mettre à jour la date de fin si fournie
        if (command.EndDate.HasValue)
        {
            // Vérifier que la nouvelle date de fin est postérieure à startDate (actuel ou modifié)
            if (command.EndDate.Value <= budget.StartDate)
            {
                throw new BadRequestException("La date de fin doit être postérieure à la date de début");
            }
            budget.EndDate = command.EndDate.Value;
        }

        // Mettre à jour le seuil d'alerte si fourni
        if (command.AlertThreshold.HasValue)
        {
            budget.AlertThreshold = command.AlertThreshold.Value;
        }

        // Mettre à jour le montant alloué si fourni
        if (command.AllocatedAmount.HasValue)
        {
            var newAllocatedAmount = command.AllocatedAmount.Value;

            // Vérifier si le nouveau montant est inférieur au montant dépensé
            if (newAllocatedAmount < budget.SpentAmount)
            {
                warning = $"Le montant alloué ({newAllocatedAmount}) est inférieur au montant dépensé ({budget.SpentAmount})";
                budget.IsExceeded = true;
            }
            else
            {
                // Recalculer isExceeded en fonction du nouveau montant
                budget.IsExceeded = false;
            }

            budget.AllocatedAmount = newAllocatedAmount;
        }

        // Gérer les catégories si fournies
        List<Guid> finalCategoryIds;
        if (command.CategoryIds != null)
        {
            // Vérifier que les nouvelles catégories existent et sont de type EXPENSE
            if (command.CategoryIds.Any())
            {
                var categories = await categoryRepository.GetByConditionAsync(
                    c => command.CategoryIds.Contains(c.Id)
                         && c.ApplicationId == command.ApplicationId
                         && c.BoutiqueId == command.BoutiqueId
                         && c.IsActive,
                    cancellationToken);

                var foundCategoryIds = categories.Select(c => c.Id).ToList();
                var missingCategoryIds = command.CategoryIds.Except(foundCategoryIds).ToList();

                if (missingCategoryIds.Any())
                {
                    throw new BadRequestException($"Les catégories suivantes n'existent pas ou ne sont pas actives: {string.Join(", ", missingCategoryIds)}");
                }

                // Vérifier que toutes les catégories sont de type EXPENSE
                var nonExpenseCategories = categories.Where(c => c.Type != CategoryType.EXPENSE).ToList();
                if (nonExpenseCategories.Any())
                {
                    throw new BadRequestException("Seules les catégories de type EXPENSE sont autorisées");
                }
            }

            // Supprimer les anciennes associations
            var existingBudgetCategories = await budgetCategoryRepository.GetByConditionAsync(
                bc => bc.BudgetId == budget.Id,
                cancellationToken);

            foreach (var bc in existingBudgetCategories)
            {
                dbContext.Set<BudgetCategory>().Remove(bc);
            }

            // Créer les nouvelles associations
            foreach (var categoryId in command.CategoryIds)
            {
                var budgetCategory = new BudgetCategory
                {
                    BudgetId = budget.Id,
                    CategoryId = categoryId
                };
                await budgetCategoryRepository.AddDataAsync(budgetCategory, cancellationToken);
            }

            finalCategoryIds = command.CategoryIds;
        }
        else
        {
            // Récupérer les catégories existantes
            var existingBudgetCategories = await budgetCategoryRepository.GetByConditionAsync(
                bc => bc.BudgetId == budget.Id,
                cancellationToken);
            finalCategoryIds = existingBudgetCategories.Select(bc => bc.CategoryId).ToList();
        }

        // Mettre à jour l'entité (synchrone)
        budgetRepository.UpdateData(budget);
        await unitOfWork.SaveChangesDataAsync(cancellationToken);

        var budgetDto = new BudgetDTO(
            Id: budget.Id,
            ApplicationId: budget.ApplicationId,
            BoutiqueId: budget.BoutiqueId,
            Name: budget.Name,
            StartDate: budget.StartDate,
            EndDate: budget.EndDate,
            AllocatedAmount: budget.AllocatedAmount,
            SpentAmount: budget.SpentAmount,
            RemainingAmount: budget.RemainingAmount,
            Currency: budget.Currency,
            Type: budget.Type,
            AlertThreshold: budget.AlertThreshold,
            IsExceeded: budget.IsExceeded,
            IsActive: budget.IsActive,
            CategoryIds: finalCategoryIds,
            CreatedAt: budget.CreatedAt,
            UpdatedAt: budget.UpdatedAt
        );

        return new UpdateBudgetResult(budgetDto, warning);
    }

    private async Task<bool> HasExpensesAsync(
        Guid budgetId,
        string applicationId,
        string boutiqueId,
        CancellationToken cancellationToken)
    {
        // Récupérer les catégories associées au budget
        var budgetCategories = await budgetCategoryRepository.GetByConditionAsync(
            bc => bc.BudgetId == budgetId,
            cancellationToken);

        if (!budgetCategories.Any())
        {
            return false;
        }

        var categoryIds = budgetCategories.Select(bc => bc.CategoryId.ToString()).ToList();

        // Vérifier s'il existe des dépenses liées à ces catégories pour cette boutique
        var expenses = await cashFlowRepository.GetByConditionAsync(
            cf => cf.ApplicationId == applicationId
                  && cf.BoutiqueId == boutiqueId
                  && categoryIds.Contains(cf.CategoryId)
                  && cf.Type == CashFlowType.EXPENSE,
            cancellationToken);

        return expenses.Any();
    }
}
