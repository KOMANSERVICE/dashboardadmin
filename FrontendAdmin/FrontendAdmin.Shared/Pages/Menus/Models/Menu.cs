namespace FrontendAdmin.Shared.Pages.Menus.Models;

public record Menu
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string ApiRoute { get; set; } = "";
    public string UrlFront { get; set; } = "";
    public string Icon { get; set; }
    public bool IsSelected { get; set; }
}
public record CreateMenuRequest(Menu Menu);
public record CreateMenuResponse(Guid Id);
public record GetMenuByApplicationResponse(List<Menu> Menus);