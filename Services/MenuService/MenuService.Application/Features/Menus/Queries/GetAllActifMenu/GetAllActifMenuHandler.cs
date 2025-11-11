namespace MenuService.Application.Features.Menus.Queries.GetAllActifMenu;

public class GetAllActifMenuHandler(    
    IGenericRepository<Menu> _menuRepository)
    : IQueryHandler<GetAllActifMenuQuery, GetAllActifMenuResult>
{
    public async Task<GetAllActifMenuResult> Handle(GetAllActifMenuQuery request, CancellationToken cancellationToken)
    {
        var AppAdminReference = request.AppAdminReference;
        var menus = await _menuRepository.GetByConditionAsync(m => m.IsActif &&
        m.AppAdminReference == AppAdminReference, cancellationToken);
        var result = menus.Select(m => m.Adapt<MenuDTO>()).ToList();

        return new GetAllActifMenuResult(result);
    }
}