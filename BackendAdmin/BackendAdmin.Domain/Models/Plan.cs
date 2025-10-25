namespace BackendAdmin.Domain.Models;

public class Plan : Entity<Guid>
{
    public Guid ApplicationId { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public bool RequiresPayment { get; set; }
    public bool IsPopular { get; set; }
    public bool IsDisplay { get; set; }
}
