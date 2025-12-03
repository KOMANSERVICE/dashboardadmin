namespace BackendAdmin.Application.Features.Menus.Commands.ActiveMenu;

public record ActiveMenuCommand(string Reference, string AppAdminReference)
    : ICommand<ActiveMenuResult>;
public record ActiveMenuResult(bool Success);
