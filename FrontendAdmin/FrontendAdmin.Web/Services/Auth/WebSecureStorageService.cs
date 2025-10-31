using FrontendAdmin.Shared.Services.Auth;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FrontendAdmin.Web.Services.Auth;

public class WebSecureStorageService(ProtectedLocalStorage _storage) : IStorageService
{
    public async Task SetAsync(string key, string value) => await _storage.SetAsync(key, value);

    public async Task<string?> GetAsync(string key)
    {
        var stored = await _storage.GetAsync<string>(key);
        return stored.Success ? stored.Value : null;
    }

    public async Task RemoveAsync(string key) => await _storage.DeleteAsync(key);
}

