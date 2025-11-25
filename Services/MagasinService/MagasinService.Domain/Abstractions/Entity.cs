using System.ComponentModel.DataAnnotations.Schema;

namespace MagasinService.Domain.Abstractions;

public abstract class Entity<T>
{
    [Column("ch1")]
    public T Id { get; set; }
}