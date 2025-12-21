namespace BackendAdmin.Application.Features.Menus.DTOs;

public record MenuInfoDTO
{
    public string Name { get; set; }
    public string Reference { get; set; }
    public string UrlFront { get; set; }
    public string Icon { get; set; }
    public string AppAdminReference { get; set; }
    public bool IsActif { get; set; }
    public string? Group { get; set; }
    public int SortOrder { get; set; }
}

public record MenuSortOrderItem(string Reference, int SortOrder);

public record ReorderMenusRequest(string AppAdminReference, List<MenuSortOrderItem> Items);

public record ReorderMenusResponse(bool Success, int UpdatedCount);

