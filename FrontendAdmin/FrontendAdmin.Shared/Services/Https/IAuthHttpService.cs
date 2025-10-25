using FrontendAdmin.Shared.Models;
using FrontendAdmin.Shared.Pages.Auths.Models;
using Refit;

namespace FrontendAdmin.Shared.Services.Https;

public interface IAuthHttpService
{
    [Post("/auth/signin")]
    Task<BaseResponse<SignInResponse>> SignIn(SignInRequest request);
}
