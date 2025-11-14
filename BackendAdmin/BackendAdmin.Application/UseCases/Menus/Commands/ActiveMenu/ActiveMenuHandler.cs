
using BackendAdmin.Application.ApiExterne.Menus;
using Mapster;

namespace BackendAdmin.Application.UseCases.Menus.Commands.ActiveMenu;

public class ActiveMenuHandler(
        IMenuService menuService
    )
    : ICommandHandler<ActiveMenuCommand, ActiveMenuResult>
{
    public async Task<ActiveMenuResult> Handle(ActiveMenuCommand request, CancellationToken cancellationToken)
    {
        var command = request.Adapt<ActiveMenuRequest>();
        var result = await menuService.ActiveMenuAsync(command);

        if(!result.Success)
            throw new BadRequestException(result.Message);

        return new ActiveMenuResult(result.Success);
    }
}
