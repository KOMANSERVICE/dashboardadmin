
namespace FrontendAdmin.Shared.Pages.Applications.Models;


public record GetAppAdminByUserResponse(List<AppAdmin> AppAdmins);
public record AppAdmin
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
}
public record CreateAppAdminRequest(AppAdmin AppAdmin);
public record CreateAppAdminResponse(Guid Id);