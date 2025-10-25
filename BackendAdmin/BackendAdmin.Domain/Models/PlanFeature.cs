namespace BackendAdmin.Domain.Models;

public class PlanFeature
{
    public Guid Id { get; set; }
    public Guid PlanId { get; set; }
    public Guid FeatureId { get; set; }
    public string Description { get; set; } = default!;
    public string Value { get; set; } = default!;
}
