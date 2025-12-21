using IDR.Library.BuildingBlocks.Contexts.Services;
using TresorerieService.Application.Features.RecurringCashFlows.DTOs;

namespace TresorerieService.Application.Features.RecurringCashFlows.Commands.UpdateRecurringCashFlow;

public class UpdateRecurringCashFlowHandler(
    IGenericRepository<RecurringCashFlow> recurringCashFlowRepository,
    IGenericRepository<Category> categoryRepository,
    IGenericRepository<Account> accountRepository,
    IUnitOfWork unitOfWork,
    IUserContextService userContextService
) : ICommandHandler<UpdateRecurringCashFlowCommand, UpdateRecurringCashFlowResult>
{
    public async Task<UpdateRecurringCashFlowResult> Handle(
        UpdateRecurringCashFlowCommand command,
        CancellationToken cancellationToken = default)
    {
        // Recuperer le flux recurrent
        var recurringCashFlows = await recurringCashFlowRepository.GetByConditionAsync(
            r => r.Id == command.Id
                 && r.ApplicationId == command.ApplicationId
                 && r.BoutiqueId == command.BoutiqueId,
            cancellationToken);

        var recurringCashFlow = recurringCashFlows.FirstOrDefault();
        if (recurringCashFlow == null)
        {
            throw new NotFoundException("Flux recurrent non trouve");
        }

        var data = command.Data;
        bool frequencyChanged = false;

        // Valider et mettre a jour le type si fourni
        if (data.Type.HasValue)
        {
            if (data.Type != CashFlowType.INCOME && data.Type != CashFlowType.EXPENSE)
            {
                throw new BadRequestException("Le type de flux doit etre INCOME ou EXPENSE");
            }
            recurringCashFlow.Type = data.Type.Value;
        }

        // Valider et mettre a jour la categorie si fournie
        Category? category = null;
        if (!string.IsNullOrEmpty(data.CategoryId))
        {
            if (!Guid.TryParse(data.CategoryId, out var categoryGuid))
            {
                throw new BadRequestException($"L'identifiant de categorie '{data.CategoryId}' n'est pas un GUID valide");
            }

            var categories = await categoryRepository.GetByConditionAsync(
                c => c.Id == categoryGuid
                     && c.ApplicationId == command.ApplicationId
                     && c.IsActive,
                cancellationToken);

            category = categories.FirstOrDefault();
            if (category == null)
            {
                throw new NotFoundException($"La categorie avec l'ID '{data.CategoryId}' n'existe pas ou n'est pas active");
            }

            // Valider que le type de categorie correspond au type de flux
            var currentType = data.Type ?? recurringCashFlow.Type;
            var expectedCategoryType = currentType == CashFlowType.INCOME ? CategoryType.INCOME : CategoryType.EXPENSE;
            if (category.Type != expectedCategoryType)
            {
                var typeLabel = currentType == CashFlowType.INCOME ? "INCOME" : "EXPENSE";
                throw new BadRequestException($"La categorie doit etre de type {typeLabel}");
            }

            recurringCashFlow.CategoryId = data.CategoryId;
        }

        // Valider et mettre a jour le compte si fourni
        Account? account = null;
        if (data.AccountId.HasValue)
        {
            var accounts = await accountRepository.GetByConditionAsync(
                a => a.Id == data.AccountId.Value
                     && a.ApplicationId == command.ApplicationId
                     && a.BoutiqueId == command.BoutiqueId
                     && a.IsActive,
                cancellationToken);

            account = accounts.FirstOrDefault();
            if (account == null)
            {
                throw new NotFoundException("Compte non trouve");
            }

            recurringCashFlow.AccountId = data.AccountId.Value;
        }

        // Mettre a jour les champs simples
        if (!string.IsNullOrEmpty(data.Label))
        {
            recurringCashFlow.Label = data.Label;
        }

        if (data.Description != null)
        {
            recurringCashFlow.Description = data.Description;
        }

        if (data.Amount.HasValue)
        {
            recurringCashFlow.Amount = data.Amount.Value;
        }

        if (!string.IsNullOrEmpty(data.PaymentMethod))
        {
            recurringCashFlow.PaymentMethod = data.PaymentMethod;
        }

        if (data.ThirdPartyName != null)
        {
            recurringCashFlow.ThirdPartyName = data.ThirdPartyName;
        }

        // Gestion de la frequence - detecter si elle a change
        if (data.Frequency.HasValue && data.Frequency != recurringCashFlow.Frequency)
        {
            recurringCashFlow.Frequency = data.Frequency.Value;
            frequencyChanged = true;
        }

        if (data.Interval.HasValue && data.Interval != recurringCashFlow.Interval)
        {
            recurringCashFlow.Interval = data.Interval.Value;
            frequencyChanged = true;
        }

        if (data.DayOfMonth.HasValue && data.DayOfMonth != recurringCashFlow.DayOfMonth)
        {
            recurringCashFlow.DayOfMonth = data.DayOfMonth.Value;
            frequencyChanged = true;
        }

        if (data.DayOfWeek.HasValue && data.DayOfWeek != recurringCashFlow.DayOfWeek)
        {
            recurringCashFlow.DayOfWeek = data.DayOfWeek.Value;
            frequencyChanged = true;
        }

        if (data.StartDate.HasValue)
        {
            recurringCashFlow.StartDate = data.StartDate.Value.ToUtc();
            frequencyChanged = true; // Recalculer aussi si startDate change
        }

        if (data.EndDate.HasValue)
        {
            // Valider que EndDate est posterieur a StartDate (existant ou nouveau)
            var startDateToCheck = data.StartDate?.ToUtc() ?? recurringCashFlow.StartDate;
            var endDateUtc = data.EndDate.Value.ToUtc();
            if (endDateUtc <= startDateToCheck)
            {
                throw new BadRequestException("La date de fin doit etre posterieure a la date de debut");
            }
            recurringCashFlow.EndDate = endDateUtc;
        }

        if (data.AutoValidate.HasValue)
        {
            recurringCashFlow.AutoValidate = data.AutoValidate.Value;
        }

        if (data.IsActive.HasValue)
        {
            recurringCashFlow.IsActive = data.IsActive.Value;
        }

        // Recalculer NextOccurrence si la frequence a change
        if (frequencyChanged)
        {
            recurringCashFlow.NextOccurrence = CalculateNextOccurrence(
                recurringCashFlow.StartDate,
                recurringCashFlow.Frequency,
                recurringCashFlow.Interval,
                recurringCashFlow.DayOfMonth,
                recurringCashFlow.DayOfWeek);
        }

        
        recurringCashFlowRepository.UpdateData(recurringCashFlow);
        await unitOfWork.SaveChangesDataAsync(cancellationToken);

        // Charger les entites liees pour le DTO de reponse
        if (category == null && !string.IsNullOrEmpty(recurringCashFlow.CategoryId))
        {
            if (Guid.TryParse(recurringCashFlow.CategoryId, out var categoryGuid))
            {
                var categories = await categoryRepository.GetByConditionAsync(
                    c => c.Id == categoryGuid,
                    cancellationToken);
                category = categories.FirstOrDefault();
            }
        }

        if (account == null)
        {
            var accounts = await accountRepository.GetByConditionAsync(
                a => a.Id == recurringCashFlow.AccountId,
                cancellationToken);
            account = accounts.FirstOrDefault();
        }

        // Construire le DTO de reponse
        var dto = new RecurringCashFlowDTO(
            Id: recurringCashFlow.Id,
            ApplicationId: recurringCashFlow.ApplicationId,
            BoutiqueId: recurringCashFlow.BoutiqueId,
            Type: recurringCashFlow.Type,
            CategoryId: recurringCashFlow.CategoryId,
            CategoryName: category?.Name ?? "",
            Label: recurringCashFlow.Label,
            Description: recurringCashFlow.Description,
            Amount: recurringCashFlow.Amount,
            AccountId: recurringCashFlow.AccountId,
            AccountName: account?.Name ?? "",
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

        return new UpdateRecurringCashFlowResult(dto);
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
