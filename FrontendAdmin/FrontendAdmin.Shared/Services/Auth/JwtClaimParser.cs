using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;

namespace FrontendAdmin.Shared.Services.Auth;

public static class JwtClaimParser
{
    public static IEnumerable<Claim> FromToken(string token)
    {
        var parts = token.Split('.');
        if (parts.Length != 3)
        {
            return Array.Empty<Claim>();
        }

        var payload = parts[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var claims = new List<Claim>();

        using var document = JsonDocument.Parse(jsonBytes);
        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in property.Value.EnumerateArray())
                {
                    claims.Add(new Claim(MapClaimType(property.Name), element.GetString() ?? string.Empty));
                }
            }
            else
            {
                claims.Add(new Claim(MapClaimType(property.Name), property.Value.GetString() ?? string.Empty));
            }
        }

        return claims;
    }

    private static string MapClaimType(string key)
    {
        return key switch
        {
            "sub" or "nameid" => ClaimTypes.NameIdentifier,
            "name" or "unique_name" => ClaimTypes.Name,
            "email" => ClaimTypes.Email,
            _ => key
        };
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        base64 = base64.Replace('-', '+').Replace('_', '/');
        return (base64.Length % 4) switch
        {
            2 => Convert.FromBase64String(base64 + "=="),
            3 => Convert.FromBase64String(base64 + "="),
            _ => Convert.FromBase64String(base64)
        };
    }
}


