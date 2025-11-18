using BackendAdmin.Application.UseCases.Menus.DTOs;

namespace BackendAdmin.Application.UseCases.Menus.Queries.GetMenuByApplication;

public record GetMenuByApplicationQuery(string AppAdminReference)
    : IQuery<GetMenuByApplicationResult>;

public record GetMenuByApplicationResult(List<MenuInfoDTO> Menus);
