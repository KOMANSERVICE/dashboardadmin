using BackendAdmin.Application.UseCases.Menus.DTOs;

namespace BackendAdmin.Application.UseCases.Menus.Commands.UpdateMenu;

public record UpdateMenuCommand(MenuInfoDTO Menu, string AppAdminReference)
    : ICommand<UpdateMenuResult>;

public record UpdateMenuResult(bool Success);
