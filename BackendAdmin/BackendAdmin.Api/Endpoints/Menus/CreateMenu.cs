using BackendAdmin.Application.UseCases.Menus.Commands.CreateMenu;
using BackendAdmin.Application.UseCases.Menus.DTOs;

namespace BackendAdmin.Api.Endpoints.Menus;


public record CreateMenuRequest(MenuInfoDTO Menu);
public record CreateMenuResponse(Guid Id);
public class CreateMenu : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/menu/{appAdminReference}", async (string appAdminReference, CreateMenuRequest request, ISender sender) =>
        {
            var command = new CreateMenuCommand(request.Menu, appAdminReference);            
            var result = await sender.Send(command);

            var response = result.Adapt<CreateMenuResponse>();
            var baseResponse = ResponseFactory.Success(response, "Menu créer avec succèss", StatusCodes.Status201Created);

            return Results.Created($"/menu/{response.Id}", baseResponse);
        })
       .WithName("CreateMenu")
       .WithTags("Menu")
       .Produces<BaseResponse<CreateMenuResponse>>(StatusCodes.Status201Created)
       .ProducesProblem(StatusCodes.Status400BadRequest)
       .WithSummary("CreateMenu")
       .WithDescription("CreateMenu")
       .RequireAuthorization()
       .WithOpenApi();
    }
}
