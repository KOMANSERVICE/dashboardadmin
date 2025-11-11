namespace MenuService.Application.Features.Menus.Queries.GetAllActifMenu;

public record GetAllActifMenuQuery(string AppAdminReference)
    : IQuery<GetAllActifMenuResult>;

public record GetAllActifMenuResult(List<MenuDTO> Menus);
