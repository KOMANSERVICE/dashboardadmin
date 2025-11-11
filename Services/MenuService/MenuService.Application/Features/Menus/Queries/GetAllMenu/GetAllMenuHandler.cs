

using Mapster;

namespace MenuService.Application.Features.Menus.Queries.GetAllMenu;

public class GetAllMenuHandler(
        IGenericRepository<Menu> _menuRepository
    ) :
    IQueryHandler<GetAllMenuQuery, GetAllMenuResult>
{
    public async Task<GetAllMenuResult> Handle(GetAllMenuQuery request, CancellationToken cancellationToken)
    {
        var menus = await _menuRepository.GetAllAsync(cancellationToken);
        var result = menus.Select(m => m.Adapt<MenuStateDto>()).ToList();
        return new GetAllMenuResult(result);
    }
}
