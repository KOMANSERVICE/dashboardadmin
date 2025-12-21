

using Mapster;

namespace MenuService.Application.Features.Menus.Queries.GetAllMenu;

public class GetAllMenuHandler(
        IGenericRepository<Menu> _menuRepository
    ) :
    IQueryHandler<GetAllMenuQuery, GetAllMenuResult>
{
    public async Task<GetAllMenuResult> Handle(GetAllMenuQuery request, CancellationToken cancellationToken)
    {
        var menus = await _menuRepository.GetByConditionAsync(
            m => m.AppAdminReference == request.AppAdminReference
            , cancellationToken);
        var result = menus.OrderBy(m => m.SortOrder).Select(m => m.Adapt<MenuStateDto>()).ToList();
        return new GetAllMenuResult(result);
    }
}
