using BackendAdmin.Application.UseCases.Menus.DTOs;

namespace BackendAdmin.Application.UseCases.Menus.Commands.CreateMenu;

public record CreateMenuCommand(MenuDTO Menu, Guid AppAdminId)
    : ICommand<CreateMenuResult>;

public record CreateMenuResult(Guid Id);