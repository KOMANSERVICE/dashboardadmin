using IDR.Library.BuildingBlocks.Repositories;
using MenuService.Application.Features.Menus.Commands.ActiveMenu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MenuService.Application.Features.Menus.Commands.InactiveMenu;

public class InactiveMenuHandler(
        IGenericRepository<Menu> _menuRepository,
        IUnitOfWork unitOfWork
    )
    : ICommandHandler<InactiveMenuCommand, InactiveMenuResult>
{
    public async Task<InactiveMenuResult> Handle(InactiveMenuCommand request, CancellationToken cancellationToken)
    {
        var menu = await UpdateMenu(request, cancellationToken);

        _menuRepository.UpdateData(menu);
        await unitOfWork.SaveChangesDataAsync();

        return new InactiveMenuResult(true);
    }


    private async Task<Menu> UpdateMenu(InactiveMenuCommand request, CancellationToken cancellationToken)
    {
        var reference = request.Reference;
        var appAdminReference = request.AppAdminReference;
        var menu = await _menuRepository.FindAsync(
            m => m.Reference == reference &&
            m.AppAdminReference == appAdminReference, cancellationToken
        );

        if (menu == null)
        {
            throw new NotFoundException($"Menu avec la réference {reference} non trouvé.");
        }

        menu.IsActif = false;

        return menu;
    }
}
