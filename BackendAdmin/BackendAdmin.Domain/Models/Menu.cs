namespace BackendAdmin.Domain.Models;

[Table("TM00001")]
public class Menu : Entity<MenuId>
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
    public AppAdminId AppAdminId { get; set; }

    public AppAdmin AppAdmin { get; set; }
}
