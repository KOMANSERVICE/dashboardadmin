using FrontendAdmin.Shared.Pages.Menus.Models;

namespace FrontendAdmin.Shared.Services.Https;

public interface IMenuHttpService
{
    [Get("/menu/{appAdminId}")]
    Task<BaseResponse<GetMenuByApplicationResponse>> GetMenuByApplicationAsync(Guid appAdminId);

    [Post("/menu/{appAdminId}")]
    Task<BaseResponse<CreateMenuResponse>> CreateMenuAsync(Guid AppAdminId, CreateMenuRequest request);
}
