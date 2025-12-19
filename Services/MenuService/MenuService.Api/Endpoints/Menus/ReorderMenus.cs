using IDR.Library.BuildingBlocks.Security.Attributes;
using MenuService.Application.Features.Menus.Commands.ReorderMenus;

namespace MenuService.Api.Endpoints.Menus;

public record ReorderMenusRequest(string AppAdminReference, List<MenuSortOrderItem> Items);
public record ReorderMenusResponse(bool Success, int UpdatedCount);

public class ReorderMenus : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/menu/reorder", [RequireScope("menu:write")] async (ReorderMenusRequest request, ISender sender) =>
        {
            var command = request.Adapt<ReorderMenusCommand>();
            var result = await sender.Send(command);

            var response = result.Adapt<ReorderMenusResponse>();
            var baseResponse = ResponseFactory.Success(response, "Menus reordonnes avec succes", StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
       .WithName("ReorderMenus")
        .WithTags("Menu")
       .Produces<BaseResponse<ReorderMenusResponse>>(StatusCodes.Status200OK)
       .ProducesProblem(StatusCodes.Status400BadRequest)
       .ProducesProblem(StatusCodes.Status401Unauthorized)
       .ProducesProblem(StatusCodes.Status403Forbidden)
       .ProducesProblem(StatusCodes.Status404NotFound)
       .WithSummary("ReorderMenus")
       .WithDescription("Reorder multiple menus by updating their SortOrder. Requires 'menu:write' scope.")
       .WithOpenApi();
    }
}
