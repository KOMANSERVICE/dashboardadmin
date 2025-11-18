using BackendAdmin.Application.UseCases.Menus.Commands.InactiveMenu;

namespace BackendAdmin.Api.Endpoints.Menus;

public record InactiveMenuRequest(string Reference, string AppAdminReference);

public record InactiveMenuResponse(bool Success);
public class InactiveMenu : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/menu/inactive", async (InactiveMenuRequest request, ISender sender) =>
        {
            var command = request.Adapt<InactiveMenuCommand>();
            var result = await sender.Send(command);

            var response = result.Adapt<InactiveMenuResponse>();
            var baseResponse = ResponseFactory.Success(response, "Liste des menus réccuperées avec succès", StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
       .WithName("InactiveMenu")
        .WithTags("Menu")
       .Produces<BaseResponse<InactiveMenuResponse>>(StatusCodes.Status200OK)
       .ProducesProblem(StatusCodes.Status400BadRequest)
       .WithSummary("InactiveMenu")
       .WithDescription("InactiveMenu")
       //.RequireAuthorization()
       .WithOpenApi();
    }
}
