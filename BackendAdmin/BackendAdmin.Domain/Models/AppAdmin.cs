using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BackendAdmin.Domain.Models;

[Table("TA00001")]
public class AppAdmin : Entity<AppAdminId>
{
    [Column("cf1")]
    public string Name { get; set; } = default!;
    [Column("cf2")]
    public string Reference { get; set; } = default!;
    [Column("cf3")]
    public string Description { get; set; } = default!;

    [Column("cf4")]
    public string Link { get; set; } = default!;
    public virtual ICollection<Menu> Menus { get; set; }
}
