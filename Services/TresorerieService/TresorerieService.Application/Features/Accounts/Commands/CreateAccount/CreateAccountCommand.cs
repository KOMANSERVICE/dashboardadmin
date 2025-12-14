using IDR.Library.BuildingBlocks.CQRS;
using TresorerieService.Application.Features.Accounts.DTOs;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Application.Features.Accounts.Commands.CreateAccount;

public record CreateAccountCommand(
    string ApplicationId,
    string BoutiqueId,
    string Name,
    string? Description,
    AccountType Type,
    decimal InitialBalance,
    decimal? AlertThreshold,
    decimal? OverdraftLimit,
    string? AccountNumber,
    string? BankName,
    bool IsDefault
) : ICommand<CreateAccountResult>;

public record CreateAccountResult(AccountDTO Account);
