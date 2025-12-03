using BackendAdmin.Application.Features.Menus.Commands.ActiveMenu;

namespace BackendAdmin.Api.Endpoints.Menus;

public record ActiveMenuRequest(string Reference, string AppAdminReference);

public record ActiveMenuResponse(bool Success);
public class ActiveMenu : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/menu/active", async (ActiveMenuRequest request, ISender sender) =>
        {
            var command = request.Adapt<ActiveMenuCommand>();
            var result = await sender.Send(command);

            var response = result.Adapt<ActiveMenuResponse>();
            var baseResponse = ResponseFactory.Success(response, "Liste des menus réccuperées avec succès", StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
       .WithName("ActiveMenu")
        .WithTags("Menu")
       .Produces<BaseResponse<ActiveMenuResponse>>(StatusCodes.Status200OK)
       .ProducesProblem(StatusCodes.Status400BadRequest)
       .WithSummary("ActiveMenu")
       .WithDescription("ActiveMenu")
       //.RequireAuthorization()
       .WithOpenApi();
    }
}
