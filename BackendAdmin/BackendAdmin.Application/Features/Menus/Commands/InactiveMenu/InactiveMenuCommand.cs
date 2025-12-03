namespace BackendAdmin.Application.Features.Menus.Commands.InactiveMenu;

public record InactiveMenuCommand(string Reference, string AppAdminReference)
    : ICommand<InactiveMenuResult>;

public record InactiveMenuResult(bool Success);