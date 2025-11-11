using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Repositories;
using MediatR;
using MenuService.Application.Features.Menus.DTOs;
using MenuService.Domain.Models;

namespace MenuService.Application.Features.Menus.Commands.CreateMenu;

public class CreateMenuHandler(
        IGenericRepository<Menu> _menuRepository,
        IUnitOfWork unitOfWork
    )
     : ICommandHandler<CreateMenuCommand, CreateMenuResult>
{
    public async Task<CreateMenuResult> Handle(CreateMenuCommand request, CancellationToken cancellationToken)
    {
        var menuDto = request.Menu;

        var menuEntity = CreateMenu(menuDto);

        await _menuRepository.AddDataAsync(menuEntity);
        await unitOfWork.SaveChangesDataAsync();


        return new CreateMenuResult(menuEntity.Id);
    }

    private static Menu CreateMenu(MenuDTO menu)
    {
        return new Menu
        {
            Name = menu.Name,
            Reference = menu.Reference,
            UrlFront = menu.UrlFront,
            Icon = menu.Icon,
            AppAdminReference = menu.AppAdminReference
        };
    }
}
