using IDR.Library.BuildingBlocks.Abstractions;
using System.ComponentModel.DataAnnotations.Schema;

namespace TresorerieService.Domain.Abstractions;

public class Entity<T> : IEntity<T>
{
    [Column("ch1")]
    public T Id { get; set; }
    [Column("ch2")]
    public DateTime CreatedAt { get ; set ; } = DateTime.UtcNow;
    [Column("ch3")]
    public DateTime UpdatedAt { get ; set ; }
    [Column("ch4")]
    public string CreatedBy { get ; set ; }
    [Column("ch5")]
    public string UpdatedBy { get ; set ; } = string.Empty;
}
