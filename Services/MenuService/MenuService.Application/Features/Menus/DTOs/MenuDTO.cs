using System.ComponentModel.DataAnnotations.Schema;

namespace MenuService.Application.Features.Menus.DTOs;

public class MenuDTO
{
    public string Name { get; set; }
    public string Reference { get; set; }
    public string UrlFront { get; set; }
    public string Icon { get; set; }
    public string AppAdminReference { get; set; }
    public string? Group { get; set; }
    public int SortOrder { get; set; }
}
