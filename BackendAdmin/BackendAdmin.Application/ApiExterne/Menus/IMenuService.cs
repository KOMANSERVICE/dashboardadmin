using BackendAdmin.Application.Features.Menus.DTOs;
using IDR.Library.Shared.Responses;
using Refit;

namespace BackendAdmin.Application.ApiExterne.Menus;

public record GetAllMenuResponse(List<MenuInfoDTO> Menus);
public record CreateMenuResponse(Guid Id);
public record CreateMenuRequest(MenuInfoDTO Menu);
public record ActiveMenuRequest(string Reference, string AppAdminReference);
public record ActiveMenuResponse(bool Success);
public record UpdateMenuRequest(MenuInfoDTO Menu);
public record UpdateMenuResponse(MenuInfoDTO Menu);
public interface IMenuService
{
    [Get("/menu/{appAdminReference}")]
    Task<BaseResponse<GetAllMenuResponse>> GetAllMenuAsync(string appAdminReference);

    [Post("/menu")]
    Task<BaseResponse<CreateMenuResponse>> CreateMenuAsync(CreateMenuRequest request);

    [Patch("/menu/active")]
    Task<BaseResponse<ActiveMenuResponse>> ActiveMenuAsync(ActiveMenuRequest request);

    [Patch("/menu/inactive")]
    Task<BaseResponse<ActiveMenuResponse>> InactiveMenuAsync(ActiveMenuRequest request);

    [Put("/menu")]
    Task<BaseResponse<UpdateMenuResponse>> UpdateMenuAsync(UpdateMenuRequest request);

    
}
