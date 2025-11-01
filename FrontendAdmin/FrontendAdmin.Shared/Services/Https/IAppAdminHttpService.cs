using FrontendAdmin.Shared.Models;
using FrontendAdmin.Shared.Pages.Applications.Models;

namespace FrontendAdmin.Shared.Services.Https;

public interface IAppAdminHttpService
{
    [Get("/application")]
    Task<BaseResponse<GetAppAdminByUserResponse>> GetAppAdminByUserAsync();
    [Post("/application")]
    Task<BaseResponse<CreateAppAdminResponse>> CreateAppAdminAsync(CreateAppAdminRequest request);
}
