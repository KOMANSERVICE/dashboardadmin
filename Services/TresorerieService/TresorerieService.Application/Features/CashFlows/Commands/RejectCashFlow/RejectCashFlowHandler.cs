using TresorerieService.Application.Features.CashFlows.DTOs;

namespace TresorerieService.Application.Features.CashFlows.Commands.RejectCashFlow;

public class RejectCashFlowHandler(
    IGenericRepository<CashFlow> cashFlowRepository,
    IGenericRepository<CashFlowHistory> cashFlowHistoryRepository,
    IGenericRepository<Category> categoryRepository,
    IGenericRepository<Account> accountRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<RejectCashFlowCommand, RejectCashFlowResult>
{
    public async Task<RejectCashFlowResult> Handle(
        RejectCashFlowCommand command,
        CancellationToken cancellationToken = default)
    {
        // Verifier que l'utilisateur est manager ou admin
        var userRole = command.UserRole.ToLower();
        if (userRole != "manager" && userRole != "admin")
        {
            throw new BadRequestException("Acces refuse: seul un manager ou admin peut rejeter un flux");
        }

        // Recuperer le flux de tresorerie
        var cashFlows = await cashFlowRepository.GetByConditionAsync(
            cf => cf.Id == command.CashFlowId
                  && cf.ApplicationId == command.ApplicationId
                  && cf.BoutiqueId == command.BoutiqueId,
            cancellationToken);

        var cashFlow = cashFlows.FirstOrDefault();
        if (cashFlow == null)
        {
            throw new NotFoundException("Flux non trouve");
        }

        // Verifier que le flux est en PENDING
        if (cashFlow.Status != CashFlowStatus.PENDING)
        {
            throw new BadRequestException("Le flux doit etre en statut PENDING pour etre rejete");
        }

        // Stocker l'ancien statut pour l'historique
        var oldStatus = cashFlow.Status.ToString();

        // Mettre a jour le statut et le motif de rejet
        cashFlow.Status = CashFlowStatus.REJECTED;
        cashFlow.RejectionReason = command.RejectionReason;
        cashFlow.UpdatedAt = DateTime.UtcNow;
        cashFlow.UpdatedBy = command.RejectedBy;

        // NE PAS modifier le solde du compte (contrairement a l'approbation)
        // NE PAS modifier le budget (contrairement a l'approbation)

        // Creer une entree dans l'historique
        var history = new CashFlowHistory
        {
            Id = Guid.NewGuid(),
            CashFlowId = cashFlow.Id.ToString(),
            Action = CashFlowAction.REJECTED,
            OldStatus = oldStatus,
            NewStatus = CashFlowStatus.REJECTED.ToString(),
            Comment = command.RejectionReason,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = command.RejectedBy,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = command.RejectedBy
        };

        // Sauvegarder les modifications
        await cashFlowHistoryRepository.AddDataAsync(history, cancellationToken);
        cashFlowRepository.UpdateData(cashFlow);
        await unitOfWork.SaveChangesDataAsync(cancellationToken);

        // Recuperer les informations supplementaires pour le DTO
        var categories = await categoryRepository.GetByConditionAsync(
            c => c.Id.ToString() == cashFlow.CategoryId,
            cancellationToken);
        var category = categories.FirstOrDefault();

        var accounts = await accountRepository.GetByConditionAsync(
            a => a.Id == cashFlow.AccountId,
            cancellationToken);
        var account = accounts.FirstOrDefault();

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
            AccountName: account?.Name ?? "Compte inconnu",
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

        return new RejectCashFlowResult(cashFlowDto);
    }
}
