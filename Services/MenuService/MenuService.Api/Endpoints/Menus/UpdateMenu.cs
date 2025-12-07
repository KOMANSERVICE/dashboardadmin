using IDR.Library.BuildingBlocks.Security.Attributes;
using MenuService.Application.Features.Menus.Commands.UpdateMenu;
using MenuService.Application.Features.Menus.DTOs;

namespace MenuService.Api.Endpoints.Menus;


public record UpdateMenuRequest(MenuDTO Menu);
public record UpdateMenuResponse(MenuDTO Menu);

public class UpdateMenu : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/menu", [RequireScope("menu:write")] async (UpdateMenuRequest request, ISender sender) =>
        {
            var command = request.Adapt<UpdateMenuCommand>();
            var result = await sender.Send(command);

            var response = result.Adapt<UpdateMenuResponse>();
            var baseResponse = ResponseFactory.Success(response, "Menu mis a jour avec succes", StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
       .WithName("UpdateMenu")
        .WithTags("Menu")
       .Produces<BaseResponse<UpdateMenuResponse>>(StatusCodes.Status200OK)
       .ProducesProblem(StatusCodes.Status400BadRequest)
       .ProducesProblem(StatusCodes.Status401Unauthorized)
       .ProducesProblem(StatusCodes.Status403Forbidden)
       .WithSummary("UpdateMenu")
       .WithDescription("Update an existing menu item. Requires 'menu:write' scope.")
       .WithOpenApi();
    }
}
