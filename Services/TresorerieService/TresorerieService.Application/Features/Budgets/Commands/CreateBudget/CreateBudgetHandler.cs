using TresorerieService.Application.Features.Budgets.DTOs;

namespace TresorerieService.Application.Features.Budgets.Commands.CreateBudget;

public class CreateBudgetHandler(
        IGenericRepository<Budget> budgetRepository,
        IGenericRepository<Category> categoryRepository,
        IGenericRepository<BudgetCategory> budgetCategoryRepository,
        IUnitOfWork unitOfWork
    )
    : ICommandHandler<CreateBudgetCommand, CreateBudgetResult>
{
    public async Task<CreateBudgetResult> Handle(
        CreateBudgetCommand command,
        CancellationToken cancellationToken = default)
    {
        // Vérifier l'unicité du nom de budget pour cette boutique dans la même période
        var existingBudgets = await budgetRepository.GetByConditionAsync(
            b => b.ApplicationId == command.ApplicationId
                 && b.BoutiqueId == command.BoutiqueId
                 && b.Name == command.Name
                 && b.IsActive,
            cancellationToken);

        if (existingBudgets.Any())
        {
            throw new BadRequestException($"Un budget avec le nom '{command.Name}' existe déjà pour cette boutique");
        }

        // Si des catégories sont spécifiées, vérifier qu'elles existent et sont de type EXPENSE
        if (command.CategoryIds != null && command.CategoryIds.Any())
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

        // Créer le budget
        var budget = new Budget
        {
            Id = Guid.NewGuid(),
            ApplicationId = command.ApplicationId,
            BoutiqueId = command.BoutiqueId,
            Name = command.Name,
            StartDate = command.StartDate,
            EndDate = command.EndDate,
            AllocatedAmount = command.AllocatedAmount,
            SpentAmount = 0,
            Currency = "USD",
            Type = command.Type,
            AlertThreshold = command.AlertThreshold,
            IsExceeded = false,
            IsActive = true
        };

        await budgetRepository.AddDataAsync(budget, cancellationToken);

        // Créer les associations Budget-Category
        if (command.CategoryIds != null && command.CategoryIds.Any())
        {
            foreach (var categoryId in command.CategoryIds)
            {
                var budgetCategory = new BudgetCategory
                {
                    BudgetId = budget.Id,
                    CategoryId = categoryId
                };
                await budgetCategoryRepository.AddDataAsync(budgetCategory, cancellationToken);
            }
        }

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
            CategoryIds: command.CategoryIds ?? new List<Guid>(),
            CreatedAt: budget.CreatedAt,
            UpdatedAt: budget.UpdatedAt
        );

        return new CreateBudgetResult(budgetDto);
    }
}
