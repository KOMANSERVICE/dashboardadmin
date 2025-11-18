using IDR.Library.Blazor.Enums;
using IDR.Library.Blazor.LocalStorages;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FrontendAdmin.Web.Services;

public class WebSecureStorageService(
        ProtectedLocalStorage _localStorage
    ) : IStorageService
{
    public async Task SetAsync(string key, string value, StorageType storageType = StorageType.Local)
    {
        await _localStorage.SetAsync(key, value);
    }

    public async Task<string?> GetAsync(string key)
    {
        var resultLocal = await _localStorage.GetAsync<string>(key);
        return resultLocal.Success ? resultLocal.Value : null;
    }

    public async Task RemoveAsync(string key) {
        await _localStorage.DeleteAsync(key);
    }
}

