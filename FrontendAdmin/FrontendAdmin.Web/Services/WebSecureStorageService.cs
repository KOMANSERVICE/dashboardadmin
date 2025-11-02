using IDR.Library.Blazor.Enums;
using IDR.Library.Blazor.LocalStorages;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FrontendAdmin.Web.Services;

public class WebSecureStorageService(
        ProtectedLocalStorage _localStorage,
        ProtectedSessionStorage _sessionStorage
    ) : IStorageService
{
    public async Task SetAsync(string key, string value, StorageType storageType = StorageType.Local)
    {
        if (storageType == StorageType.Local)
            await _localStorage.SetAsync(key, value);
        else
            await _sessionStorage.SetAsync(key, value);
    }

    public async Task<string?> GetAsync(string key)
    {
        var resultLocal = await _localStorage.GetAsync<string>(key);
        if (resultLocal.Success)
            return resultLocal.Value;

        var resultSession = await _sessionStorage.GetAsync<string>(key);
        return resultSession.Success ? resultSession.Value : null;
    }

    public async Task RemoveAsync(string key) {
        await _localStorage.DeleteAsync(key);
        await _sessionStorage.DeleteAsync(key);
    }
}

