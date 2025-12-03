using BackendAdmin.Application.Features.Menus.DTOs;

namespace BackendAdmin.Application.Features.Menus.Commands.CreateMenu;

public record CreateMenuCommand(MenuInfoDTO Menu, string AppAdminReference)
    : ICommand<CreateMenuResult>;

public record CreateMenuResult(Guid Id);