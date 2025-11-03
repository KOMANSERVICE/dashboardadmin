namespace BackendAdmin.Domain.Models;
[Table("TR00001")]
public class RefreshToken
{
    [Column("fc1")]
    public Guid Id { get; set; }
    [Column("fc2")]
    public string TokenHash { get; set; } = default!;
    [Column("fc3")]
    public string Email { get; set; } = default!;
    [Column("fc4")]
    public string UserId { get; set; } = default!;
    [Column("fc5")]
    public string Role { get; set; } = default!;
    [Column("fc6")]
    public DateTime CreatedAt { get; set; }
    [Column("fc7")]
    public DateTime? ExpiresAt { get; set; }
    [Column("fc8")]
    public bool IsRevoked { get; set; }
    [Column("fc9")]
    public string? RevokedReason { get; set; }
}
