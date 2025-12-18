using MenuService.Domain.Abstractions;
using System.ComponentModel.DataAnnotations.Schema;

namespace MenuService.Domain.Models;

[Table("TM00001")]
public class Menu : Entity<Guid>
{

    [Column("cf1")]
    public string Name { get; set; } = default!;
    [Column("cf2")]
    public string Reference { get; set; } = default!;

    [Column("cf3")]
    public string UrlFront { get; set; } = default!;
    [Column("cf4")]
    public string Icon { get; set; } = default!;
    [Column("cf5")]
    public bool IsActif { get; set; }
    [Column("cf6")]
    public string AppAdminReference { get; set; } = default!;
    [Column("cf7")]
    public string? Group { get; set; }
}
