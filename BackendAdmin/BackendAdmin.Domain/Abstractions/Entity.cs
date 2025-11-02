
using IDR.Library.BuildingBlocks.Abstractions;

namespace BackendAdmin.Domain.Abstractions;

public abstract class Entity<T> : IEntity<T>
{
    [Column("ch1")]
    public T Id { get; set; }
    [Column("ch2")]
    public DateTime CreatedAt { get; set; }
    [Column("ch3")]
    public DateTime UpdatedAt { get; set; }
    [Column("ch4")]
    public string CreatedBy { get; set; } = string.Empty;
    [Column("ch5")]
    public string UpdatedBy { get; set; } = string.Empty;
}
