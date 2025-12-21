namespace TresorerieService.Application.Features.CashFlows.Commands.CreateTransfer;

public record TransferDto(
    Guid Id,
    string Type,
    string Status,
    Guid AccountId,
    string AccountName,
    Guid DestinationAccountId,
    string DestinationAccountName,
    decimal Amount,
    DateTime Date,
    string Label,
    string? Description,
    decimal SourceAccountBalance,
    decimal DestinationAccountBalance,
    DateTime CreatedAt,
    string CreatedBy
);
