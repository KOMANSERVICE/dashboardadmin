using BackendAdmin.Application.UseCases.Menus.DTOs;
using IDR.Library.Shared.Responses;
using Refit;

namespace BackendAdmin.Application.ApiExterne.Menus;

public record GetAllMenuResponse(List<MenuInfoDTO> Menus);
public interface IMenuService
{
    [Get("/menu/{appAdminReference}")]
    Task<BaseResponse<GetAllMenuResponse>> GetAllMenuAsync(string appAdminReference);
}
