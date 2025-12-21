using IDR.Library.BuildingBlocks.Contexts.Services;
using TresorerieService.Application.Features.RecurringCashFlows.DTOs;

namespace TresorerieService.Application.Features.RecurringCashFlows.Commands.CreateRecurringCashFlow;

public class CreateRecurringCashFlowHandler(
    IGenericRepository<RecurringCashFlow> recurringCashFlowRepository,
    IGenericRepository<Category> categoryRepository,
    IGenericRepository<Account> accountRepository,
    IUnitOfWork unitOfWork,
    IUserContextService userContextService
) : ICommandHandler<CreateRecurringCashFlowCommand, CreateRecurringCashFlowResult>
{
    public async Task<CreateRecurringCashFlowResult> Handle(
        CreateRecurringCashFlowCommand command,
        CancellationToken cancellationToken = default)
    {
        // Valider que le type est INCOME ou EXPENSE (pas TRANSFER)
        if (command.Type != CashFlowType.INCOME && command.Type != CashFlowType.EXPENSE)
        {
            throw new BadRequestException($"Le type de flux doit etre INCOME ou EXPENSE. Type fourni: {command.Type}");
        }

        // Valider le format du CategoryId
        if (!Guid.TryParse(command.CategoryId, out var categoryGuid))
        {
            throw new BadRequestException($"L'identifiant de categorie '{command.CategoryId}' n'est pas un GUID valide");
        }

        // Valider que la categorie existe et est du bon type
        var categories = await categoryRepository.GetByConditionAsync(
            c => c.Id == categoryGuid
                 && c.ApplicationId == command.ApplicationId
                 && c.IsActive,
            cancellationToken);

        var category = categories.FirstOrDefault();
        if (category == null)
        {
            throw new NotFoundException($"La categorie avec l'ID '{command.CategoryId}' n'existe pas ou n'est pas active");
        }

        // Valider que le type de categorie correspond au type de flux
        var expectedCategoryType = command.Type == CashFlowType.INCOME ? CategoryType.INCOME : CategoryType.EXPENSE;
        if (category.Type != expectedCategoryType)
        {
            var typeLabel = command.Type == CashFlowType.INCOME ? "INCOME" : "EXPENSE";
            throw new BadRequestException($"La categorie doit etre de type {typeLabel}");
        }

        // Valider que le compte existe et appartient a la boutique
        var accounts = await accountRepository.GetByConditionAsync(
            a => a.Id == command.AccountId
                 && a.ApplicationId == command.ApplicationId
                 && a.BoutiqueId == command.BoutiqueId
                 && a.IsActive,
            cancellationToken);

        var account = accounts.FirstOrDefault();
        if (account == null)
        {
            throw new NotFoundException("Compte non trouve");
        }

        // Convertir les dates en UTC
        var startDateUtc = command.StartDate.ToUtc();
        var endDateUtc = command.EndDate?.ToUtc();

        // Calculer la prochaine occurrence
        var nextOccurrence = CalculateNextOccurrence(
            startDateUtc,
            command.Frequency,
            command.Interval,
            command.DayOfMonth,
            command.DayOfWeek);

        // Creer le flux recurrent
        var recurringCashFlow = new RecurringCashFlow
        {
            Id = Guid.NewGuid(),
            ApplicationId = command.ApplicationId,
            BoutiqueId = command.BoutiqueId,
            Type = command.Type,
            CategoryId = command.CategoryId,
            Label = command.Label,
            Description = command.Description,
            Amount = command.Amount,
            AccountId = command.AccountId,
            PaymentMethod = command.PaymentMethod,
            ThirdPartyName = command.ThirdPartyName,
            Frequency = command.Frequency,
            Interval = command.Interval,
            DayOfMonth = command.DayOfMonth,
            DayOfWeek = command.DayOfWeek,
            StartDate = startDateUtc,
            EndDate = endDateUtc,
            NextOccurrence = nextOccurrence,
            AutoValidate = command.AutoValidate,
            IsActive = true,
            LastGeneratedAt = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            };

        await recurringCashFlowRepository.AddDataAsync(recurringCashFlow, cancellationToken);
        await unitOfWork.SaveChangesDataAsync(cancellationToken);

        // Construire le DTO de reponse
        var dto = new RecurringCashFlowDTO(
            Id: recurringCashFlow.Id,
            ApplicationId: recurringCashFlow.ApplicationId,
            BoutiqueId: recurringCashFlow.BoutiqueId,
            Type: recurringCashFlow.Type,
            CategoryId: recurringCashFlow.CategoryId,
            CategoryName: category.Name,
            Label: recurringCashFlow.Label,
            Description: recurringCashFlow.Description,
            Amount: recurringCashFlow.Amount,
            AccountId: recurringCashFlow.AccountId,
            AccountName: account.Name,
            PaymentMethod: recurringCashFlow.PaymentMethod,
            ThirdPartyName: recurringCashFlow.ThirdPartyName,
            Frequency: recurringCashFlow.Frequency,
            Interval: recurringCashFlow.Interval,
            DayOfMonth: recurringCashFlow.DayOfMonth,
            DayOfWeek: recurringCashFlow.DayOfWeek,
            StartDate: recurringCashFlow.StartDate,
            EndDate: recurringCashFlow.EndDate,
            NextOccurrence: recurringCashFlow.NextOccurrence,
            AutoValidate: recurringCashFlow.AutoValidate,
            IsActive: recurringCashFlow.IsActive,
            LastGeneratedAt: recurringCashFlow.LastGeneratedAt,
            CreatedAt: recurringCashFlow.CreatedAt,
            CreatedBy: recurringCashFlow.CreatedBy
        );

        return new CreateRecurringCashFlowResult(dto);
    }

    /// <summary>
    /// Calcule la prochaine occurrence basee sur la frequence et les parametres.
    /// La premiere occurrence est la date de debut elle-meme.
    /// </summary>
    private static DateTime CalculateNextOccurrence(
        DateTime startDate,
        RecurringFrequency frequency,
        int interval,
        int? dayOfMonth,
        int? dayOfWeek)
    {
        // La premiere occurrence est la date de debut
        // Ajuster selon la frequence si necessaire
        var nextDate = startDate;

        switch (frequency)
        {
            case RecurringFrequency.DAILY:
                // Pour DAILY, la prochaine occurrence est simplement startDate
                break;

            case RecurringFrequency.WEEKLY:
                // Pour WEEKLY, ajuster au jour de la semaine specifie
                if (dayOfWeek.HasValue)
                {
                    // dayOfWeek: 1=Lundi, 7=Dimanche
                    // DayOfWeek .NET: Sunday=0, Monday=1, ..., Saturday=6
                    var targetDotNetDayOfWeek = dayOfWeek.Value == 7 ? DayOfWeek.Sunday : (DayOfWeek)dayOfWeek.Value;
                    var currentDayOfWeek = nextDate.DayOfWeek;

                    // Calculer le nombre de jours a ajouter pour atteindre le jour cible
                    var daysUntilTarget = ((int)targetDotNetDayOfWeek - (int)currentDayOfWeek + 7) % 7;
                    if (daysUntilTarget == 0 && nextDate.Date >= startDate.Date)
                    {
                        // On est deja au bon jour
                    }
                    else if (daysUntilTarget == 0)
                    {
                        daysUntilTarget = 7; // Aller a la semaine suivante
                    }
                    nextDate = nextDate.AddDays(daysUntilTarget);
                }
                break;

            case RecurringFrequency.MONTHLY:
                // Pour MONTHLY, ajuster au jour du mois specifie
                if (dayOfMonth.HasValue)
                {
                    var targetDay = Math.Min(dayOfMonth.Value, DateTime.DaysInMonth(nextDate.Year, nextDate.Month));
                    if (nextDate.Day <= targetDay)
                    {
                        // Le jour cible est dans le mois courant
                        nextDate = new DateTime(nextDate.Year, nextDate.Month, targetDay,
                            nextDate.Hour, nextDate.Minute, nextDate.Second, DateTimeKind.Utc);
                    }
                    else
                    {
                        // Le jour cible est passe, aller au mois suivant
                        nextDate = nextDate.AddMonths(1);
                        targetDay = Math.Min(dayOfMonth.Value, DateTime.DaysInMonth(nextDate.Year, nextDate.Month));
                        nextDate = new DateTime(nextDate.Year, nextDate.Month, targetDay,
                            nextDate.Hour, nextDate.Minute, nextDate.Second, DateTimeKind.Utc);
                    }
                }
                break;

            case RecurringFrequency.QUARTERLY:
                // Pour QUARTERLY, la prochaine occurrence est simplement startDate
                // Les occurrences suivantes seront startDate + 3 mois * interval
                break;

            case RecurringFrequency.YEARLY:
                // Pour YEARLY, la prochaine occurrence est simplement startDate
                // Les occurrences suivantes seront startDate + 1 an * interval
                break;
        }

        return nextDate;
    }
}
