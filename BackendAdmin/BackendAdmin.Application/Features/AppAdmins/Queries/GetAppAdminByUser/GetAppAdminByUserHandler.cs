
using BackendAdmin.Application.Data;
using BackendAdmin.Application.Features.AppAdmins.DTOs;
using IDR.Library.BuildingBlocks.Security.Interfaces;

namespace BackendAdmin.Application.Features.AppAdmins.Queries.GetAppAdminByUser;

public class GetAppAdminByUserHandler(
        IConfiguration _configuration,
        IApplicationDbContext _applicationDbContext,
        IUserContextService _userContextService,
        ISecureSecretProvider _secureSecretProvider
    )
    : IQueryHandler<GetAppAdminByUserQuery, GetAppAdminByUserResult>
{
    public async Task<GetAppAdminByUserResult> Handle(GetAppAdminByUserQuery request, CancellationToken cancellationToken)
    {
        string userId = _userContextService.GetUserId();

        var Vault_EmailAdmin = _configuration["Vault:EmailAdmin"]!;
        var EmailAdmin = await _secureSecretProvider.GetSecretAsync(Vault_EmailAdmin);

        List<AppAdminDTO> appAdmins = new List<AppAdminDTO>();

        if (userId.Equals(EmailAdmin))
        {
            appAdmins = await _applicationDbContext.
                Applications
                .Select(a => new AppAdminDTO
                {
                    Id = a.Id.Value,
                    Name = a.Name,
                    Reference = a.Reference,
                    Description = a.Description,
                    Link = a.Link
                })
                .ToListAsync();
        }

        return new GetAppAdminByUserResult(appAdmins);
    }
}
