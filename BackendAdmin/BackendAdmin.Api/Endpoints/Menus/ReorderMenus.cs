using BackendAdmin.Application.Features.Menus.Commands.ReorderMenus;
using BackendAdmin.Application.Features.Menus.DTOs;

namespace BackendAdmin.Api.Endpoints.Menus;

public record ReorderMenusApiRequest(List<MenuSortOrderItem> Items);
public record ReorderMenusApiResponse(bool Success, int UpdatedCount);

public class ReorderMenus : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/menu/{appAdminReference}/reorder", async (string appAdminReference, ReorderMenusApiRequest request, ISender sender) =>
        {
            var command = new ReorderMenusCommand(appAdminReference, request.Items);
            var result = await sender.Send(command);

            var response = result.Adapt<ReorderMenusApiResponse>();
            var baseResponse = ResponseFactory.Success(response, "Menus reordonnes avec succes", StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
       .WithName("ReorderMenus")
        .WithTags("Menu")
       .Produces<BaseResponse<ReorderMenusApiResponse>>(StatusCodes.Status200OK)
       .ProducesProblem(StatusCodes.Status400BadRequest)
       .ProducesProblem(StatusCodes.Status404NotFound)
       .WithSummary("ReorderMenus")
       .WithDescription("Reorder menus by updating their SortOrder")
       .RequireAuthorization();
    }
}
