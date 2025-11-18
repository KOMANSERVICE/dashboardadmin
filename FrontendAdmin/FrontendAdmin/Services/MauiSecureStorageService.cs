using IDR.Library.Blazor.Enums;
using IDR.Library.Blazor.LocalStorages;
using System;
namespace FrontendAdmin.Services;

public class MauiSecureStorageService : IStorageService
{
    public Task SetAsync(string key, string value, StorageType storageType = StorageType.Local)
    {
        Preferences.Set(key, value);
        return Task.CompletedTask;
    }
    public Task<string?> GetAsync(string key)
    {
        var value = Preferences.Get(key, null);
        return Task.FromResult<string?>(value);
    }
    public Task RemoveAsync(string key)
    {
        Preferences.Remove(key);
        return Task.CompletedTask;
    }
}