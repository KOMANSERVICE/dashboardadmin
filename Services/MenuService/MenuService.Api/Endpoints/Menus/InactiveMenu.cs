using IDR.Library.BuildingBlocks.Security.Attributes;
using MenuService.Application.Features.Menus.Commands.InactiveMenu;

namespace MenuService.Api.Endpoints.Menus;

public record InactiveMenuRequest(string Reference, string AppAdminReference);

public record InactiveMenuResponse(bool Success);
public class InactiveMenu : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/menu/inactive", [RequireScope("menu:admin")] async (InactiveMenuRequest request, ISender sender) =>
        {
            var command = request.Adapt<InactiveMenuCommand>();
            var result = await sender.Send(command);

            var response = result.Adapt<InactiveMenuResponse>();
            var baseResponse = ResponseFactory.Success(response, "Menu desactive avec succes", StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
       .WithName("InactiveMenu")
        .WithTags("Menu")
       .Produces<BaseResponse<InactiveMenuResponse>>(StatusCodes.Status200OK)
       .ProducesProblem(StatusCodes.Status400BadRequest)
       .ProducesProblem(StatusCodes.Status401Unauthorized)
       .ProducesProblem(StatusCodes.Status403Forbidden)
       .WithSummary("InactiveMenu")
       .WithDescription("Deactivate a menu item. Requires 'menu:admin' scope.")
       .WithOpenApi();
    }
}

