namespace FrontendAdmin.Shared.Pages.Auths.Models;

public class SignIn
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}
public record SignInRequest(SignIn Signin);
public record SignInResponse(string Token);