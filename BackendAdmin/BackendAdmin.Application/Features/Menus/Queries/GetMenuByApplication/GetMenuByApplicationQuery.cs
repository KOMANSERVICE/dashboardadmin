using BackendAdmin.Application.Features.Menus.DTOs;

namespace BackendAdmin.Application.Features.Menus.Queries.GetMenuByApplication;

public record GetMenuByApplicationQuery(string AppAdminReference)
    : IQuery<GetMenuByApplicationResult>;

public record GetMenuByApplicationResult(List<MenuInfoDTO> Menus);
