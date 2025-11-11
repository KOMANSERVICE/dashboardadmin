namespace MenuService.Application.Features.Menus.Queries.GetAllMenu;

public class GetAllMenuQuery()
    : IQuery<GetAllMenuResult>;

public record GetAllMenuResult(List<MenuStateDto> Menus);
