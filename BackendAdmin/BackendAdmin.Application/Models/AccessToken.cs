namespace BackendAdmin.Application.Models;

public class AccessToken
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;

}