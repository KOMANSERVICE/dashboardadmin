namespace MenuService.Application.Features.Menus.Queries.GetAllActifMenu;

public class GetAllActifMenuHandler(    
    IGenericRepository<Menu> _menuRepository)
    : IQueryHandler<GetAllActifMenuQuery, GetAllActifMenuResult>
{
    public async Task<GetAllActifMenuResult> Handle(GetAllActifMenuQuery request, CancellationToken cancellationToken)
    {

        var menus = await _menuRepository.GetByConditionAsync(m => m.IsActif ,cancellationToken);
        var result = menus.Select(m => m.Adapt<MenuDTO>()).ToList();

        return new GetAllActifMenuResult(result);
    }
}