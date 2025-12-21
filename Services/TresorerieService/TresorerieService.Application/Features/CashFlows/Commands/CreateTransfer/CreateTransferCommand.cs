namespace TresorerieService.Application.Features.CashFlows.Commands.CreateTransfer;

public record CreateTransferCommand(
    string ApplicationId,
    string BoutiqueId,
    Guid AccountId,
    Guid DestinationAccountId,
    decimal Amount,
    DateTime Date,
    string Label,
    string? Description,
    string CreatedBy
) : ICommand<CreateTransferResult>;

public record CreateTransferResult(
    TransferDto Transfer
);
