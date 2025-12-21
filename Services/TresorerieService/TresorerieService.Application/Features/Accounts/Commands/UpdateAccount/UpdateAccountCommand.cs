using IDR.Library.BuildingBlocks.CQRS;
using TresorerieService.Application.Features.Accounts.DTOs;

namespace TresorerieService.Application.Features.Accounts.Commands.UpdateAccount;

public record UpdateAccountCommand(
    Guid AccountId,
    string ApplicationId,
    string BoutiqueId,
    UpdateAccountDTO Data
) : ICommand<UpdateAccountResult>;

public record UpdateAccountResult(Guid Id);
