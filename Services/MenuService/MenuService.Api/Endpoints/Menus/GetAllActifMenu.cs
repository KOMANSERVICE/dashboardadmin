using MenuService.Application.Features.Menus.DTOs;
using MenuService.Application.Features.Menus.Queries.GetAllActifMenu;

namespace MenuService.Api.Endpoints.Menus;

public record GetAllActifMenuResponse(List<MenuStateDto> Menus);

public class GetAllActifMenu : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/menu/{appAdminReference}/actif", async (string appAdminReference, ISender sender) =>
        {
            var result = await sender.Send(new GetAllActifMenuQuery(appAdminReference));

            var response = result.Adapt<GetAllActifMenuResponse>();
            var baseResponse = ResponseFactory.Success(response, "Liste des menus réccuperées avec succès", StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
       .WithName("GetAllActifMenu")
        .WithTags("Menu")
       .Produces<BaseResponse<GetAllActifMenuResponse>>(StatusCodes.Status200OK)
       .ProducesProblem(StatusCodes.Status400BadRequest)
       .WithSummary("GetAllActifMenu")
       .WithDescription("GetAllActifMenu")
       //.RequireAuthorization()
       .WithOpenApi();
    }
}

