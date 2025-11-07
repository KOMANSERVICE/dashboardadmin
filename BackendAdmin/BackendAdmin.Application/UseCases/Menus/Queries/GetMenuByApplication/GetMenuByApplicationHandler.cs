
using BackendAdmin.Application.Data;
using BackendAdmin.Application.UseCases.Menus.DTOs;
using BackendAdmin.Domain.ValueObjects;

namespace BackendAdmin.Application.UseCases.Menus.Queries.GetMenuByApplication;

public class GetMenuByApplicationHandler(
        IApplicationDbContext _dbContext
    )
    : IQueryHandler<GetMenuByApplicationQuery, GetMenuByApplicationResult>
{
    public async Task<GetMenuByApplicationResult> Handle(GetMenuByApplicationQuery request, CancellationToken cancellationToken)
    {
        var appAdmin = request.AppAdminId;
        var menus = await _dbContext.Applications
            .Where(a => a.Id == AppAdminId.Of(appAdmin))
            .Include(a => a.Menus)
            .SelectMany(a => a.Menus)
            .Select(m => new MenuDTO(
                    m.Id.Value,
                    m.Name,
                    m.ApiRoute,
                    m.UrlFront,
                    m.Icon
                ))
            .ToListAsync();

        return new GetMenuByApplicationResult(menus);
    }
}
