namespace BackendAdmin.Domain.Models;

public class ApplicationSetting : Entity<Guid>
{
    public Guid ApplicationId { get; set; }
    public string Key { get; set; } = default!;
    public string Value { get; set; } = default!;
}
