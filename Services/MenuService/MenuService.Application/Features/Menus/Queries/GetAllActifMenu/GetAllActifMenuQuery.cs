namespace MenuService.Application.Features.Menus.Queries.GetAllActifMenu;

public class GetAllActifMenuQuery
    : IQuery<GetAllActifMenuResult>;

public record GetAllActifMenuResult(List<MenuDTO> Menus);
