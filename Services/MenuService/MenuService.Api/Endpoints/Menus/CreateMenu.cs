using IDR.Library.BuildingBlocks.Security.Attributes;
using MenuService.Application.Features.Menus.Commands.CreateMenu;
using MenuService.Application.Features.Menus.DTOs;

namespace MenuService.Api.Endpoints.Menus;


public record CreateMenuRequest(MenuDTO Menu);
public record CreateMenuResponse(Guid Id);
public class CreateMenu : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/menu", [RequireScope("menu:write")] async (CreateMenuRequest request, ISender sender) =>
        {
            var command = new CreateMenuCommand(request.Menu);
            var result = await sender.Send(command);

            var response = result.Adapt<CreateMenuResponse>();
            var baseResponse = ResponseFactory.Success(response, "Menu cree avec succes", StatusCodes.Status201Created);

            return Results.Created($"/menu/{response.Id}", baseResponse);
        })
       .WithName("CreateMenu")
       .WithTags("Menu")
       .Produces<BaseResponse<CreateMenuResponse>>(StatusCodes.Status201Created)
       .ProducesProblem(StatusCodes.Status400BadRequest)
       .ProducesProblem(StatusCodes.Status401Unauthorized)
       .ProducesProblem(StatusCodes.Status403Forbidden)
       .WithSummary("CreateMenu")
       .WithDescription("Create a new menu item. Requires 'menu:write' scope.")
       .WithOpenApi();
    }
}

