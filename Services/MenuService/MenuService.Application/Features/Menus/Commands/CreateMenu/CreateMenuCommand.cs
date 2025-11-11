using IDR.Library.BuildingBlocks.CQRS;
using MenuService.Application.Features.Menus.DTOs;

namespace MenuService.Application.Features.Menus.Commands.CreateMenu;

public record CreateMenuCommand(MenuDTO Menu)
    : ICommand<CreateMenuResult>;

public record CreateMenuResult(Guid Id);
