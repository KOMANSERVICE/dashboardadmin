namespace BackendAdmin.Domain.Models;

public class PlanMenu : Entity<Guid>
{
    public Guid PlanId { get; set; }
    public Guid MenuId { get; set; }
}
