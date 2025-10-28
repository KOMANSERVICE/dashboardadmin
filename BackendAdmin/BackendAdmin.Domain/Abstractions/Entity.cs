
namespace BackendAdmin.Domain.Abstractions;

public abstract class Entity<T> : IEntity<T>
{
    [Column("champ1")]
    public T Id { get; set; }
    [Column("champ2")]
    public DateTime CreatedAt { get; set; }
    [Column("champ3")]
    public DateTime UpdatedAt { get; set; }
    [Column("champ4")]
    public string CreatedBy { get; set; } = string.Empty;
    [Column("champ5")]
    public string UpdatedBy { get; set; } = string.Empty;
}
