using BackendAdmin.Application.UseCases.Menus.DTOs;

namespace BackendAdmin.Application.UseCases.Menus.Queries.GetMenuByApplication;

public record GetMenuByApplicationQuery(Guid AppAdminId)
    : IQuery<GetMenuByApplicationResult>;

public record GetMenuByApplicationResult(List<MenuDTO> Menus);
