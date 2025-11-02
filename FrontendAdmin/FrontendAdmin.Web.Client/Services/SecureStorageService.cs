using Blazored.LocalStorage;
using Blazored.SessionStorage;
using IDR.Library.Blazor.Enums;
using IDR.Library.Blazor.LocalStorages;

namespace FrontendAdmin.Web.Client.Services;

public class SecureStorageService(
        ILocalStorageService _localStorage,
        ISessionStorageService _sessionStorage
    ) : IStorageService
{
    public async Task SetAsync(string key, string value, StorageType storageType = StorageType.Local)
    {
        if (storageType == StorageType.Local)
            await _localStorage.SetItemAsync(key, value);
        else
            await _sessionStorage.SetItemAsync(key, value);

    }
    public async Task<string?> GetAsync(string key)
    {
        return await _localStorage.GetItemAsync<string>(key) ??
             await _sessionStorage.GetItemAsync<string>(key);
    }

    public async Task RemoveAsync(string key)
    {
        await _localStorage.RemoveItemAsync(key);
        await _sessionStorage.RemoveItemAsync(key);
    }
}