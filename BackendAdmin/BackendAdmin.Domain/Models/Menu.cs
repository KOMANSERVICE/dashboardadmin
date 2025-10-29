namespace BackendAdmin.Domain.Models;

[Table("TM00001")]
public class Menu : Entity<MenuId>
{
    [Column("cf1")]
    public Guid ApplicationId { get; set; }
    [Column("cf2")]
    public string Name { get; set; } = default!;
    [Column("cf3")]
    public string ApiRoute { get; set; } = default!;

    [Column("cf4")]
    public string UrlFront { get; set; } = default!;
    [Column("cf5")]
    public string Icon { get; set; } = default!;
    [Column("cf6")]
    public AppAdminId AppAdminId { get; set; }

    public AppAdmin AppAdmin { get; set; } = new();
}
