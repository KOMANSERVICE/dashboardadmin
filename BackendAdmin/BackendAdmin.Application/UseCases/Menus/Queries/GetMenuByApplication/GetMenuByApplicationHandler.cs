using BackendAdmin.Application.ApiExterne.Menus;
using BackendAdmin.Application.Data;

namespace BackendAdmin.Application.UseCases.Menus.Queries.GetMenuByApplication;

public class GetMenuByApplicationHandler(
        IApplicationDbContext _dbContext,
        IMenuService menuService
    )
    : IQueryHandler<GetMenuByApplicationQuery, GetMenuByApplicationResult>
{
    public async Task<GetMenuByApplicationResult> Handle(GetMenuByApplicationQuery request, CancellationToken cancellationToken)
    {

        var reference = request.AppAdminReference;
        //var menus = await _dbContext.Applications
        //    .Where(a => a.Id == AppAdminId.Of(appAdmin))
        //    .Include(a => a.Menus)
        //    .SelectMany(a => a.Menus)
        //    .Select(m => new MenuDTO(
        //            m.Id.Value,
        //            m.Name,
        //            m.ApiRoute,
        //            m.UrlFront,
        //            m.Icon
        //        ))
        //    .ToListAsync();

        var menus = await menuService.GetAllMenuAsync(reference);
        if(!menus.Success)
        {
            throw new BadRequestException(menus.Message);
        }

        return new GetMenuByApplicationResult(menus.Data.Menus);
    }
}
