using BackendAdmin.Application.UseCases.AppAdmins.DTOs;

namespace BackendAdmin.Application.UseCases.AppAdmins.Queries.GetAppAdminByUser;

public record GetAppAdminByUserQuery() 
    : IQuery<GetAppAdminByUserResult>;
public record GetAppAdminByUserResult(List<AppAdminDTO> AppAdmins);