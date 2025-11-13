
using BackendAdmin.Application.ApiExterne.Menus;
using BackendAdmin.Application.UseCases.Menus.DTOs;
using BackendAdmin.Domain.ValueObjects;

namespace BackendAdmin.Application.UseCases.Menus.Commands.CreateMenu;

public class CreateMenuHandler(
        IGenericRepository<Menu> _menuRepository,
        IUnitOfWork _unitOfWork,
        IMenuService menuService
    )
    : ICommandHandler<CreateMenuCommand, CreateMenuResult>
{
    public async Task<CreateMenuResult> Handle(CreateMenuCommand request, CancellationToken cancellationToken)
    {
        var menuDto = request.Menu;
        menuDto.AppAdminReference = request.AppAdminReference;

        var menuResponse = await menuService.CreateMenuAsync(new CreateMenuRequest(menuDto));
        if (!menuResponse.Success) { 
            throw new BadRequestException(menuResponse.Message);
        }
        //var menu = CreateMenu(menuDto, applicationId);
        //await _menuRepository.AddDataAsync(menu, cancellationToken);
        //await _unitOfWork.SaveChangesDataAsync(cancellationToken);
        var id = menuResponse.Data.Id;
        return new CreateMenuResult(id);
    }


    private Menu CreateMenu(MenuDTO menuDTO, Guid appAdminId)
    {
        var menuId = MenuId.Of(Guid.NewGuid());
        return new Menu
        {
            Id= menuId,
            Name = menuDTO.Name,
            Icon = menuDTO.Icon,
            UrlFront = menuDTO.UrlFront,
            ApiRoute = menuDTO.ApiRoute,
            AppAdminId = AppAdminId.Of(appAdminId)
        };
    }
}
