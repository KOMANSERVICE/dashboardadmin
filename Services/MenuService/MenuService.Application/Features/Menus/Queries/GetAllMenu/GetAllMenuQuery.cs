namespace MenuService.Application.Features.Menus.Queries.GetAllMenu;

public record GetAllMenuQuery(string AppAdminReference)
    : IQuery<GetAllMenuResult>;

public record GetAllMenuResult(List<MenuStateDto> Menus);
