using BackendAdmin.Application.Features.AppAdmins.DTOs;

namespace BackendAdmin.Application.Features.AppAdmins.Queries.GetAppAdminByUser;

public record GetAppAdminByUserQuery() 
    : IQuery<GetAppAdminByUserResult>;
public record GetAppAdminByUserResult(List<AppAdminDTO> AppAdmins);