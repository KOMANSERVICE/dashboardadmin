namespace MenuService.Application.Features.Menus.Commands.UpdateMenu;

public record UpdateMenuCommand(MenuDTO Menu)
    : ICommand<UpdateMenuResult>;

public record UpdateMenuResult(MenuDTO Menu);
