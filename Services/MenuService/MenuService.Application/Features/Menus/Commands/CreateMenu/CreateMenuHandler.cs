using MenuService.Application.Data;

namespace MenuService.Application.Features.Menus.Commands.CreateMenu;

public class CreateMenuHandler(
        IGenericRepository<Menu> _menuRepository,
        IMenuDbContext _dbContext,
        IUnitOfWork unitOfWork
    )
     : ICommandHandler<CreateMenuCommand, CreateMenuResult>
{
    public async Task<CreateMenuResult> Handle(CreateMenuCommand request, CancellationToken cancellationToken)
    {
        var menuDto = request.Menu;

        var menuexists = await _dbContext.Menus.AnyAsync(m => 
        m.Reference == menuDto.Reference 
        || m.Name == menuDto.Name);
        if (menuexists)
        {
            throw new BadRequestException("Menu avec la même réference ou nom existe déjà.");
        }

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
            AppAdminReference = menu.AppAdminReference,
            Group = menu.Group
        };
    }
}
