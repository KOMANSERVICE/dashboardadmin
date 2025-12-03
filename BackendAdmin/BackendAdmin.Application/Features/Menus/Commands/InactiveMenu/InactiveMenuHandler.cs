
using BackendAdmin.Application.ApiExterne.Menus;
using Mapster;

namespace BackendAdmin.Application.Features.Menus.Commands.InactiveMenu;

public class InactiveMenuHandler(
        IMenuService menuService
    )
    : ICommandHandler<InactiveMenuCommand, InactiveMenuResult>
{
    public async Task<InactiveMenuResult> Handle(InactiveMenuCommand request, CancellationToken cancellationToken)
    {
        var command = request.Adapt<ActiveMenuRequest>();
        var result = await menuService.InactiveMenuAsync(command);

        if(!result.Success)
            throw new BadRequestException(result.Message);

        return new InactiveMenuResult(result.Success);
    }
}
