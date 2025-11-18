using MenuService.Application.Features.Menus.Commands.UpdateMenu;
using MenuService.Application.Features.Menus.DTOs;

namespace MenuService.Api.Endpoints.Menus;


public record UpdateMenuRequest(MenuDTO Menu);
public record UpdateMenuResponse(MenuDTO Menu);

public class UpdateMenu : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/menu", async (UpdateMenuRequest request, ISender sender) =>
        {
            var command = request.Adapt<UpdateMenuCommand>();
            var result = await sender.Send(command);

            var response = result.Adapt<UpdateMenuResponse>();
            var baseResponse = ResponseFactory.Success(response, "Liste des menus réccuperées avec succès", StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
       .WithName("UpdateMenu")
        .WithTags("Menu")
       .Produces<BaseResponse<UpdateMenuResponse>>(StatusCodes.Status200OK)
       .ProducesProblem(StatusCodes.Status400BadRequest)
       .WithSummary("UpdateMenu")
       .WithDescription("UpdateMenu")
       //.RequireAuthorization()
       .WithOpenApi();
    }
}
