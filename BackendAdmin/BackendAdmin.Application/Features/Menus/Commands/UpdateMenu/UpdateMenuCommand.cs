using BackendAdmin.Application.Features.Menus.DTOs;

namespace BackendAdmin.Application.Features.Menus.Commands.UpdateMenu;

public record UpdateMenuCommand(MenuInfoDTO Menu, string AppAdminReference)
    : ICommand<UpdateMenuResult>;

public record UpdateMenuResult(bool Success);
