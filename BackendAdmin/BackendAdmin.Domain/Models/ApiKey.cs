namespace BackendAdmin.Domain.Models;

/// <summary>
/// Entite API Key pour l'authentification service-to-service
/// Structure similaire a RefreshToken
/// </summary>
[Table("TK00001")]
public class ApiKey
{
    [Column("kc1")]
    public Guid Id { get; set; }

    [Column("kc2")]
    public string ApiKeyHash { get; set; } = default!;

    [Column("kc3")]
    public string ApplicationId { get; set; } = default!;

    [Column("kc4")]
    public string ApplicationName { get; set; } = default!;

    [Column("kc5")]
    public string Scopes { get; set; } = default!;

    [Column("kc6")]
    public DateTime CreatedAt { get; set; }

    [Column("kc7")]
    public DateTime? ExpiresAt { get; set; }

    [Column("kc8")]
    public bool IsRevoked { get; set; }

    [Column("kc9")]
    public string? RevokedReason { get; set; }

    [Column("kc10")]
    public DateTime? RevokedAt { get; set; }

    [Column("kc11")]
    public DateTime? LastUsedAt { get; set; }
}
