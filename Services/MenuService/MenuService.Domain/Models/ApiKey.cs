using IDR.Library.BuildingBlocks.Abstractions;
using System.ComponentModel.DataAnnotations.Schema;

namespace MenuService.Domain.Models;

/// <summary>
/// API Key entity for service-to-service authentication.
/// Implements IApiKey from IDR.Library.BuildingBlocks.
/// </summary>
[Table("TM00002")]
public class ApiKey : IApiKey
{
    [Column("ch1")]
    public Guid Id { get; set; }

    [Column("cf1")]
    public string ApiKeyHash { get; set; } = default!;

    [Column("cf2")]
    public string ApplicationId { get; set; } = default!;

    [Column("cf3")]
    public string ApplicationName { get; set; } = default!;

    [Column("cf4")]
    public string Scopes { get; set; } = default!;

    [Column("cf5")]
    public DateTime CreatedAt { get; set; }

    [Column("cf6")]
    public DateTime? ExpiresAt { get; set; }

    [Column("cf7")]
    public bool IsRevoked { get; set; }

    [Column("cf8")]
    public string? RevokedReason { get; set; }

    [Column("cf9")]
    public DateTime? RevokedAt { get; set; }

    [Column("cf10")]
    public DateTime? LastUsedAt { get; set; }

    // IEntity properties required by IApiKey interface
    [Column("cf11")]
    public DateTime UpdatedAt { get; set; }

    [Column("cf12")]
    public string CreatedBy { get; set; } = string.Empty;

    [Column("cf13")]
    public string UpdatedBy { get; set; } = string.Empty;
}
