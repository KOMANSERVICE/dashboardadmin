namespace FrontendAdmin.Shared.Pages.ApiKeys.Models;

/// <summary>
/// Request to create a new API Key
/// </summary>
public record CreateApiKeyRequest
{
    public string ApplicationId { get; init; } = string.Empty;
    public string ApplicationName { get; init; } = string.Empty;
    public string[] Scopes { get; init; } = [];
    public DateTime? ExpiresAt { get; init; }
}

/// <summary>
/// Response after creating an API Key (contains the key in plain text - shown only once)
/// </summary>
public record ApiKeyCreatedResponse
{
    public string ApiKey { get; init; } = string.Empty;
    public string ApiKeyHash { get; init; } = string.Empty;
    public string ApplicationId { get; init; } = string.Empty;
    public string ApplicationName { get; init; } = string.Empty;
    public string[] Scopes { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
}

/// <summary>
/// API Key information (without the plain text key)
/// </summary>
public record ApiKeyInfo
{
    public string ApiKeyHash { get; init; } = string.Empty;
    public string ApplicationId { get; init; } = string.Empty;
    public string ApplicationName { get; init; } = string.Empty;
    public string[] Scopes { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public bool IsRevoked { get; init; }
    public string? RevokedReason { get; init; }
    public DateTime? LastUsedAt { get; init; }

    /// <summary>
    /// Returns true if the API Key is expired
    /// </summary>
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;

    /// <summary>
    /// Returns the status display text
    /// </summary>
    public string StatusText => IsRevoked ? "Revoquee" : IsExpired ? "Expiree" : "Active";

    /// <summary>
    /// Returns the CSS class for status badge
    /// </summary>
    public string StatusClass => IsRevoked ? "bg-red-100 text-red-800" : IsExpired ? "bg-yellow-100 text-yellow-800" : "bg-green-100 text-green-800";
}

/// <summary>
/// Response containing list of API Keys
/// </summary>
public record GetApiKeysResponse(List<ApiKeyInfo> ApiKeys);

/// <summary>
/// Request to revoke an API Key
/// </summary>
public record RevokeApiKeyRequest
{
    public string ApiKeyHash { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
}

/// <summary>
/// Request to rotate an API Key
/// </summary>
public record RotateApiKeyRequest
{
    public string CurrentApiKeyHash { get; init; } = string.Empty;
    public int GracePeriodDays { get; init; } = 7;
}

/// <summary>
/// Available scopes for API Keys
/// </summary>
public static class ApiKeyScopes
{
    public const string ProductsRead = "products:read";
    public const string ProductsWrite = "products:write";
    public const string ProductsAdmin = "products:admin";
    public const string OrdersRead = "orders:read";
    public const string OrdersWrite = "orders:write";
    public const string OrdersAdmin = "orders:admin";
    public const string MenusRead = "menus:read";
    public const string MenusWrite = "menus:write";
    public const string MenusAdmin = "menus:admin";
    public const string All = "*";

    public static readonly List<ScopeOption> AllScopes = new()
    {
        new(ProductsRead, "Produits - Lecture", "Permet de lire les produits"),
        new(ProductsWrite, "Produits - Ecriture", "Permet de creer et modifier les produits"),
        new(ProductsAdmin, "Produits - Admin", "Acces complet aux produits"),
        new(OrdersRead, "Commandes - Lecture", "Permet de lire les commandes"),
        new(OrdersWrite, "Commandes - Ecriture", "Permet de creer et modifier les commandes"),
        new(OrdersAdmin, "Commandes - Admin", "Acces complet aux commandes"),
        new(MenusRead, "Menus - Lecture", "Permet de lire les menus"),
        new(MenusWrite, "Menus - Ecriture", "Permet de creer et modifier les menus"),
        new(MenusAdmin, "Menus - Admin", "Acces complet aux menus"),
        new(All, "Tous les droits", "Acces complet a toutes les ressources")
    };
}

public record ScopeOption(string Value, string Label, string Description);
