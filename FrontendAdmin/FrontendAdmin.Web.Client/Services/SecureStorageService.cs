using Blazored.LocalStorage;
using Blazored.SessionStorage;
using IDR.Library.Blazor.Enums;
using IDR.Library.Blazor.LocalStorages;

namespace FrontendAdmin.Web.Client.Services;

public class SecureStorageService(
        ILocalStorageService _localStorage
    ) : IStorageService
{
    public async Task SetAsync(string key, string value, StorageType storageType = StorageType.Local)
    {
            await _localStorage.SetItemAsync(key, value);

    }
    public async Task<string?> GetAsync(string key)
    {
        return await _localStorage.GetItemAsync<string>(key) ;
    }

    public async Task RemoveAsync(string key)
    {
        await _localStorage.RemoveItemAsync(key);
    }
}