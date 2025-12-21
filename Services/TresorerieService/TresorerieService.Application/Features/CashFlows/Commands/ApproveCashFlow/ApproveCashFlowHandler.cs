using IDR.Library.BuildingBlocks.Contexts.Services;
using TresorerieService.Application.Features.CashFlows.DTOs;

namespace TresorerieService.Application.Features.CashFlows.Commands.ApproveCashFlow;

public class ApproveCashFlowHandler(
    IGenericRepository<CashFlow> cashFlowRepository,
    IGenericRepository<CashFlowHistory> cashFlowHistoryRepository,
    IGenericRepository<Category> categoryRepository,
    IGenericRepository<Account> accountRepository,
    IUnitOfWork unitOfWork,
    IUserContextService userContextService
) : ICommandHandler<ApproveCashFlowCommand, ApproveCashFlowResult>
{
    public async Task<ApproveCashFlowResult> Handle(
        ApproveCashFlowCommand command,
        CancellationToken cancellationToken = default)
    {
        // Verifier que l'utilisateur est manager ou admin
        //TODO: pas bien gerer je vais moi meme m'en occuper plus tard
        //var userRole = command.UserRole.ToLower();
        //if (userRole != "manager" && userRole != "admin")
        //{
        //    throw new BadRequestException("Acces refuse: seul un manager ou admin peut valider un flux");
        //}

        var email = userContextService.GetEmail();

        // Recuperer le flux de tresorerie
        var cashFlows = await cashFlowRepository.GetByConditionAsync(
            cf => cf.Id == command.CashFlowId
                  && cf.ApplicationId == command.ApplicationId
                  && cf.BoutiqueId == command.BoutiqueId,
            cancellationToken);

        var cashFlow = cashFlows.FirstOrDefault();
        if (cashFlow == null)
        {
            throw new NotFoundException($"Le flux de tresorerie avec l'ID '{command.CashFlowId}' n'existe pas");
        }

        // Verifier que le flux est en PENDING
        if (cashFlow.Status != CashFlowStatus.PENDING)
        {
            throw new BadRequestException($"Seul un flux en attente (PENDING) peut etre valide. Statut actuel: {cashFlow.Status}");
        }

        // Verifier que le flux n'est pas un TRANSFER (deja approuve automatiquement)
        if (cashFlow.Type == CashFlowType.TRANSFER)
        {
            throw new BadRequestException("Les transferts sont approuves automatiquement et ne passent pas par ce workflow");
        }

        // Recuperer le compte associe
        var accounts = await accountRepository.GetByConditionAsync(
            a => a.Id == cashFlow.AccountId,
            cancellationToken);
        var account = accounts.FirstOrDefault();

        if (account == null)
        {
            throw new NotFoundException($"Le compte avec l'ID '{cashFlow.AccountId}' n'existe pas");
        }

        // Stocker l'ancien statut pour l'historique
        var oldStatus = cashFlow.Status.ToString();

        // Mettre a jour le statut et les informations de validation
        cashFlow.Status = CashFlowStatus.APPROVED;
        cashFlow.ValidatedAt = DateTime.UtcNow;
        cashFlow.ValidatedBy = email;

        // Mettre a jour le solde du compte selon le type de flux
        if (cashFlow.Type == CashFlowType.INCOME)
        {
            // Crediter le compte pour un revenu
            account.CurrentBalance += cashFlow.Amount;
        }
        else if (cashFlow.Type == CashFlowType.EXPENSE)
        {
            // Debiter le compte pour une depense
            account.CurrentBalance -= cashFlow.Amount;

            // TODO: Mettre a jour le budget dans une prochaine US
            // L'entite Budget est commentee dans CashFlow.cs (lignes 111-112)
            // Une fois l'entite Budget creee, on pourra:
            // 1. Recuperer le budget associe a cette categorie/periode
            // 2. Incrementer le montant consomme du budget
            // 3. Verifier les alertes de depassement
        }

        // Creer une entree dans l'historique
        var history = new CashFlowHistory
        {
            Id = Guid.NewGuid(),
            CashFlowId = cashFlow.Id.ToString(),
            Action = CashFlowAction.APPROVED,
            OldStatus = oldStatus,
            NewStatus = CashFlowStatus.APPROVED.ToString(),
            Comment = "Flux valide par " + email
        };

        // Sauvegarder les modifications
        await cashFlowHistoryRepository.AddDataAsync(history, cancellationToken);
        cashFlowRepository.UpdateData(cashFlow);
        accountRepository.UpdateData(account);
        await unitOfWork.SaveChangesDataAsync(cancellationToken);

        // Recuperer les informations supplementaires pour le DTO
        var categories = await categoryRepository.GetByConditionAsync(
            c => c.Id.ToString() == cashFlow.CategoryId,
            cancellationToken);
        var category = categories.FirstOrDefault();

        string? destinationAccountName = null;
        if (cashFlow.DestinationAccountId.HasValue)
        {
            var destAccounts = await accountRepository.GetByConditionAsync(
                a => a.Id == cashFlow.DestinationAccountId.Value,
                cancellationToken);
            var destAccount = destAccounts.FirstOrDefault();
            destinationAccountName = destAccount?.Name;
        }

        // Construire le DTO de reponse
        var cashFlowDto = new CashFlowDTO(
            Id: cashFlow.Id,
            ApplicationId: cashFlow.ApplicationId,
            BoutiqueId: cashFlow.BoutiqueId,
            Reference: cashFlow.Reference,
            Type: cashFlow.Type,
            Status: cashFlow.Status,
            CategoryId: cashFlow.CategoryId,
            CategoryName: category?.Name ?? "Categorie inconnue",
            Label: cashFlow.Label,
            Description: cashFlow.Description,
            Amount: cashFlow.Amount,
            TaxAmount: cashFlow.TaxAmount,
            TaxRate: cashFlow.TaxRate,
            Currency: cashFlow.Currency,
            AccountId: cashFlow.AccountId,
            AccountName: account.Name,
            DestinationAccountId: cashFlow.DestinationAccountId,
            DestinationAccountName: destinationAccountName,
            PaymentMethod: cashFlow.PaymentMethod,
            Date: cashFlow.Date,
            ThirdPartyType: cashFlow.ThirdPartyType,
            ThirdPartyName: cashFlow.ThirdPartyName,
            ThirdPartyId: cashFlow.ThirdPartyId,
            AttachmentUrl: cashFlow.AttachmentUrl,
            CreatedAt: cashFlow.CreatedAt,
            CreatedBy: cashFlow.CreatedBy,
            SubmittedAt: cashFlow.SubmittedAt,
            SubmittedBy: cashFlow.SubmittedBy,
            ValidatedAt: cashFlow.ValidatedAt,
            ValidatedBy: cashFlow.ValidatedBy,
            RejectionReason: cashFlow.RejectionReason
        );

        return new ApproveCashFlowResult(cashFlowDto, account.CurrentBalance);
    }
}
