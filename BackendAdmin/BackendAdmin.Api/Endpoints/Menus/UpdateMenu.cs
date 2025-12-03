using BackendAdmin.Application.Features.Menus.Commands.UpdateMenu;
using BackendAdmin.Application.Features.Menus.DTOs;

namespace BackendAdmin.Api.Endpoints.Menus;
public record UpdateMenuRequest(MenuInfoDTO Menu, string AppAdminReference);
public record UpdateMenuResponse(bool Success);

public class UpdateMenu : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/menu/{appAdminReference}", async (string appAdminReference,UpdateMenuRequest request, ISender sender) =>
        {
            var command = new UpdateMenuCommand(request.Menu, appAdminReference);
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