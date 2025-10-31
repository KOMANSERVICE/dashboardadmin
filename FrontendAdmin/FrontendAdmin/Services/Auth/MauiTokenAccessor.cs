using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FrontendAdmin.Shared.Services.Auth;
using Microsoft.Maui.Storage;

namespace FrontendAdmin.Services.Auth;

public class MauiTokenAccessor : IServerTokenAccessor
{
    private const string TokenKey = "authToken";

    public async Task<(string? Token, IEnumerable<Claim> Claims)> GetTokenAsync()
    {
        try
        {
            var token = await SecureStorage.Default.GetAsync(TokenKey);
            if (string.IsNullOrWhiteSpace(token))
            {
                return (null, Array.Empty<Claim>());
            }

            var claims = JwtClaimParser.FromToken(token);
            return (token, claims);
        }
        catch
        {
            SecureStorage.Default.Remove(TokenKey);
            return (null, Array.Empty<Claim>());
        }
    }

    public async Task SetTokenAsync(string token, IEnumerable<Claim> claims)
    {
        await SecureStorage.Default.SetAsync(TokenKey, token);
    }

    public Task ClearTokenAsync()
    {
        SecureStorage.Default.Remove(TokenKey);
        return Task.CompletedTask;
    }
}
