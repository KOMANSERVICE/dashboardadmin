using IDR.Library.BuildingBlocks.Security.Attributes;
using MenuService.Application.Features.Menus.DTOs;
using MenuService.Application.Features.Menus.Queries.GetAllActifMenu;

namespace MenuService.Api.Endpoints.Menus;

public record GetAllActifMenuResponse(List<MenuStateDto> Menus);

public class GetAllActifMenu : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/menu/{appAdminReference}/actif", [RequireScope("menu:read")] async (string appAdminReference, ISender sender) =>
        {
            var result = await sender.Send(new GetAllActifMenuQuery(appAdminReference));

            var response = result.Adapt<GetAllActifMenuResponse>();
            var baseResponse = ResponseFactory.Success(response, "Liste des menus recuperees avec succes", StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
       .WithName("GetAllActifMenu")
        .WithTags("Menu")
       .Produces<BaseResponse<GetAllActifMenuResponse>>(StatusCodes.Status200OK)
       .ProducesProblem(StatusCodes.Status400BadRequest)
       .ProducesProblem(StatusCodes.Status401Unauthorized)
       .ProducesProblem(StatusCodes.Status403Forbidden)
       .WithSummary("GetAllActifMenu")
       .WithDescription("Get all active menus for an application. Requires 'menu:read' scope.")
       .WithOpenApi();
    }
}

