using Blazored.LocalStorage;
using IDR.Library.Blazor.LocalStorages;

namespace FrontendAdmin.Web.Client.Services;

public class SecureStorageService(ILocalStorageService _storage) : IStorageService
{
    public async Task SetAsync(string key, string value) => await _storage.SetItemAsync(key, value);

    public async Task<string?> GetAsync(string key)
    {
        return await _storage.GetItemAsync<string>(key);
    }

    public async Task RemoveAsync(string key) => await _storage.RemoveItemAsync(key);
}