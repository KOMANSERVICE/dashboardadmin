
namespace BackendAdmin.Domain.Models;

public class Feature : Entity<Guid>
{
    public string Key { get; set; } = default!;
}
