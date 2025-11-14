using FrontendAdmin.Shared.Pages.Menus.Models;

namespace FrontendAdmin.Shared.Services.Https;

public interface IMenuHttpService
{
    [Get("/menu/{appAdminReference}")]
    Task<BaseResponse<GetMenuByApplicationResponse>> GetMenuByApplicationAsync(string appAdminReference);

    [Post("/menu/{appAdminReference}")]
    Task<BaseResponse<CreateMenuResponse>> CreateMenuAsync(string appAdminReference, CreateMenuRequest request);

    [Patch("/menu/active")]
    Task<BaseResponse<ActiveMenuResponse>> ActiveMenuAsync(ActiveMenuRequest request);

    [Patch("/menu/inactive")]
    Task<BaseResponse<ActiveMenuResponse>> InactiveMenuAsync(ActiveMenuRequest request);

    [Put("/menu/{appAdminReference}")]
    Task<BaseResponse<UpdateMenuResponse>> UpdateMenuAsync(string appAdminReference, UpdateMenuRequest request);
}
