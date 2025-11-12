
using MenuService.Application.Features.Menus.DTOs;
using MenuService.Application.Features.Menus.Queries.GetAllMenu;

namespace MenuService.Api.Endpoints.Menus;


public record GetAllMenuResponse(List<MenuStateDto> Menus);

public class GetAllMenu : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/menu/{appAdminReference}", async (string appAdminReference, ISender sender) =>
        {
            var result = await sender.Send(new GetAllMenuQuery(appAdminReference));

            var response = result.Adapt<GetAllMenuResponse>();
            var baseResponse = ResponseFactory.Success(response, "Liste des menus réccuperées avec succès", StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
       .WithName("GetAllMenu")
        .WithTags("Menu")
       .Produces<BaseResponse<GetAllMenuResponse>>(StatusCodes.Status200OK)
       .ProducesProblem(StatusCodes.Status400BadRequest)
       .WithSummary("GetAllMenu")
       .WithDescription("GetAllMenu")
       //.RequireAuthorization()
       .WithOpenApi();
    }
}

