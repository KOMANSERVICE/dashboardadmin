using Menu.Grpc.Abstractions;
using System.ComponentModel.DataAnnotations.Schema;

namespace Menu.Grpc.Models;

[Table("TM00001")]
public class Menu : Entity<Guid>
{

    [Column("cf1")]
    public string Name { get; set; } = default!;
    [Column("cf2")]
    public string ApiRoute { get; set; } = default!;

    [Column("cf3")]
    public string UrlFront { get; set; } = default!;
    [Column("cf4")]
    public string Icon { get; set; } = default!;
    [Column("cf6")]
    public bool IsActif { get; set; }
    [Column("cf5")]
    public Guid AppAdminId { get; set; }
}
