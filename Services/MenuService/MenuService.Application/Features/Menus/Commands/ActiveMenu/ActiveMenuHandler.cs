

namespace MenuService.Application.Features.Menus.Commands.ActiveMenu;

public class ActiveMenuHandler(
        IGenericRepository<Menu> _menuRepository,
        IUnitOfWork unitOfWork
    )
    : ICommandHandler<ActiveMenuCommand, ActiveMenuResult>
{
    public async Task<ActiveMenuResult> Handle(ActiveMenuCommand request, CancellationToken cancellationToken)
    {
        var menu = await UpdateMenu(request, cancellationToken);

        _menuRepository.UpdateData(menu);
        await unitOfWork.SaveChangesDataAsync();

        return new ActiveMenuResult(true);
    }

    private async Task<Menu> UpdateMenu(ActiveMenuCommand request, CancellationToken cancellationToken)
    {
        var reference = request.Reference;
        var appAdminReference = request.AppAdminReference;
        var menu = await _menuRepository.FindAsync(
            m => m.Reference == reference &&
            m.AppAdminReference == appAdminReference, cancellationToken
        );

        if(menu == null)
        {
            throw new NotFoundException($"Menu avec la réference {reference} non trouvé.");
        }

        menu.IsActif = true;

        return menu;
    }
}
