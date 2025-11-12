namespace BackendAdmin.Application.UseCases.Menus.DTOs;

public record MenuDTO(Guid Id, string Name, string ApiRoute, string UrlFront, string Icon);
public record MenuInfoDTO
{
    public string Name { get; set; }
    public string Reference { get; set; }
    public string UrlFront { get; set; }
    public string Icon { get; set; }
    public string AppAdminReference { get; set; }
    public bool IsActive { get; set; }
}

