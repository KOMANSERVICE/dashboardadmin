using BackendAdmin.Application.Features.Menus.DTOs;

namespace BackendAdmin.Application.Features.Menus.Commands.ReorderMenus;

public record ReorderMenusCommand(string AppAdminReference, List<MenuSortOrderItem> Items)
    : ICommand<ReorderMenusResult>;

public record ReorderMenusResult(bool Success, int UpdatedCount);
