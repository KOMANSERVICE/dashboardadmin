using BackendAdmin.Application.ApiExterne.Menus;
using BackendAdmin.Application.Features.Menus.DTOs;

namespace BackendAdmin.Application.Features.Menus.Commands.ReorderMenus;

public record ReorderMenusHandler(
        IMenuService menuService
    )
    : ICommandHandler<ReorderMenusCommand, ReorderMenusResult>
{
    public async Task<ReorderMenusResult> Handle(ReorderMenusCommand request, CancellationToken cancellationToken)
    {
        var reorderRequest = new ReorderMenusRequest(request.AppAdminReference, request.Items);
        var result = await menuService.ReorderMenusAsync(reorderRequest);

        if (!result.Success)
            throw new BadRequestException(result.Message);

        return new ReorderMenusResult(result.Success, result.Data.UpdatedCount);
    }
}
