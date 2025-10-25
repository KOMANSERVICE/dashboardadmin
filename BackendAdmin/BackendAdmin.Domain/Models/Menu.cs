namespace BackendAdmin.Domain.Models;

[Table("TM00001")]
public class Menu : Entity<MenuId>
{
    public Guid ApplicationId { get; set; }
    public string Name { get; set; } = default!;
    public string ApiRoute { get; set; } = default!;
    public string UrlFront { get; set; } = default!;
    public string Icon { get; set; } = default!;
}
