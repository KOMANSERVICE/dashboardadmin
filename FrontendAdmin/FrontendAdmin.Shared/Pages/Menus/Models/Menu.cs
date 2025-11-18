namespace FrontendAdmin.Shared.Pages.Menus.Models;

public record Menu{
    public string Name { get; set; }
    public string Reference { get; set; }
    public string UrlFront { get; set; }
    public string Icon { get; set; }
    public string AppAdminReference { get; set; }
    public bool IsActif { get; set; }
    public bool IsSelected { get; set; }
}
public record CreateMenuRequest(Menu Menu);
public record CreateMenuResponse(Guid Id);
public record GetMenuByApplicationResponse(List<Menu> Menus);
public record ActiveMenuRequest(string Reference, string AppAdminReference);
public record ActiveMenuResponse(bool Success);
public record UpdateMenuRequest(Menu Menu);
public record UpdateMenuResponse(bool Success);