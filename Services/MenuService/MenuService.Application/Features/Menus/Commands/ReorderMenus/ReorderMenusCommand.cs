namespace MenuService.Application.Features.Menus.Commands.ReorderMenus;

public record ReorderMenusCommand(string AppAdminReference, List<MenuSortOrderItem> Items)
    : ICommand<ReorderMenusResult>;

public record MenuSortOrderItem(string Reference, int SortOrder);

public record ReorderMenusResult(bool Success, int UpdatedCount);
