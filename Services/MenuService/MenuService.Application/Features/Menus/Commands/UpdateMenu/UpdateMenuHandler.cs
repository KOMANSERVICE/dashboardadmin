using MenuService.Application.Data;
using MenuService.Application.Features.Menus.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MenuService.Application.Features.Menus.Commands.UpdateMenu;

public class UpdateMenuHandler(
        IGenericRepository<Menu> _menuRepository,
        IMenuDbContext _dbContext,
        IUnitOfWork unitOfWork
    )
    : ICommandHandler<UpdateMenuCommand, UpdateMenuResult>
{
    public async Task<UpdateMenuResult> Handle(UpdateMenuCommand request, CancellationToken cancellationToken)
    {

        var menuDto = request.Menu;

        var menuEntity = await UpdateMenu(menuDto, cancellationToken);
        _menuRepository.UpdateData(menuEntity);
        await unitOfWork.SaveChangesDataAsync();

        var menuResultDto = menuEntity.Adapt<MenuDTO>();

        return new UpdateMenuResult(menuResultDto);
    }

    private async Task<Menu> UpdateMenu(MenuDTO menudto, CancellationToken cancellationToken)
    {
        var menu = await _menuRepository.FindAsync(
            m => m.Reference == menudto.Reference && 
            m.AppAdminReference == menudto.AppAdminReference, cancellationToken
        );

        if(menu == null)
        {
            throw new NotFoundException($"Menu with reference {menudto.Reference} not found.");
        }

        var menuexists = await _dbContext.Menus.AnyAsync(m =>
        m.Reference != menudto.Reference
        && m.Name == menudto.Name);
        if (menuexists)
        {
            throw new BadRequestException("Menu avec la même réference ou nom existe déjà.");
        }

        menu = MapToEntity(menudto, menu);

        return menu;
    }

    private static Menu MapToEntity(MenuDTO menuDto, Menu entity)
    {
        entity.Name = menuDto.Name;
        entity.UrlFront = menuDto.UrlFront;
        entity.Icon = menuDto.Icon;
        return entity;
    }
}
