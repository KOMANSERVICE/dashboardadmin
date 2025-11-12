namespace FrontendAdmin.Shared.Pages.Menus.Models;

public record Menu{
    public string Name;
    public string Reference;
    public string UrlFront;
    public string Icon;
    public string AppAdminReference;
    public bool IsActive;
    public bool IsSelected;
}
public record CreateMenuRequest(Menu Menu);
public record CreateMenuResponse(Guid Id);
public record GetMenuByApplicationResponse(List<Menu> Menus);