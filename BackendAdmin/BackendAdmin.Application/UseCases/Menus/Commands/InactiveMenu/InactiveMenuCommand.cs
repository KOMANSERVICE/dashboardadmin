namespace BackendAdmin.Application.UseCases.Menus.Commands.InactiveMenu;

public record InactiveMenuCommand(string Reference, string AppAdminReference)
    : ICommand<InactiveMenuResult>;

public record InactiveMenuResult(bool Success);