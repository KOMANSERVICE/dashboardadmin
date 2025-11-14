
using BackendAdmin.Application.ApiExterne.Menus;
using BackendAdmin.Application.UseCases.Menus.DTOs;
using Mapster;

namespace BackendAdmin.Application.UseCases.Menus.Commands.UpdateMenu;

public record UpdateMenuHandler(
        IMenuService menuService
    )
    : ICommandHandler<UpdateMenuCommand, UpdateMenuResult>
{
    public async Task<UpdateMenuResult> Handle(UpdateMenuCommand request, CancellationToken cancellationToken)
    {
        var menu = request.Menu;
        menu.AppAdminReference = request.AppAdminReference;


        var result = await menuService.UpdateMenuAsync(new UpdateMenuRequest(menu));

        if(!result.Success)
            throw new BadRequestException(result.Message);

        return new UpdateMenuResult(result.Success);
    }
}
