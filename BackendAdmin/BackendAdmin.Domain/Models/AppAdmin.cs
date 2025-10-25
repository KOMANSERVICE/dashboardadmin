using System.ComponentModel.DataAnnotations.Schema;

namespace BackendAdmin.Domain.Models;

[Table("TA00001")]
public class AppAdmin : Entity<ApplicationId>
{
    public string Name { get; set; } = default!;
    public string Reference { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Link { get; set; } = default!;
}
