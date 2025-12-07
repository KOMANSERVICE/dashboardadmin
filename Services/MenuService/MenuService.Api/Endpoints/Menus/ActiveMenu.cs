using IDR.Library.BuildingBlocks.Security.Attributes;
using MenuService.Application.Features.Menus.Commands.ActiveMenu;

namespace MenuService.Api.Endpoints.Menus;


public record ActiveMenuRequest(string Reference, string AppAdminReference);

public record ActiveMenuResponse(bool Success);
public class ActiveMenu : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/menu/active", [RequireScope("menu:admin")] async (ActiveMenuRequest request, ISender sender) =>
        {
            var command = request.Adapt<ActiveMenuCommand>();
            var result = await sender.Send(command);

            var response = result.Adapt<ActiveMenuResponse>();
            var baseResponse = ResponseFactory.Success(response, "Menu active avec succes", StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
       .WithName("ActiveMenu")
        .WithTags("Menu")
       .Produces<BaseResponse<ActiveMenuResponse>>(StatusCodes.Status200OK)
       .ProducesProblem(StatusCodes.Status400BadRequest)
       .ProducesProblem(StatusCodes.Status401Unauthorized)
       .ProducesProblem(StatusCodes.Status403Forbidden)
       .WithSummary("ActiveMenu")
       .WithDescription("Activate a menu item. Requires 'menu:admin' scope.")
       .WithOpenApi();
    }
}

