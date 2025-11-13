using BackendAdmin.Application.UseCases.Menus.DTOs;

namespace BackendAdmin.Application.UseCases.Menus.Commands.CreateMenu;

public record CreateMenuCommand(MenuInfoDTO Menu, string AppAdminReference)
    : ICommand<CreateMenuResult>;

public record CreateMenuResult(Guid Id);