namespace FrontendAdmin.Shared.Services.Auth;

public interface IStorageService
{
    Task SetAsync(string key, string value);
    Task<string?> GetAsync(string key);
    Task RemoveAsync(string key);
}
