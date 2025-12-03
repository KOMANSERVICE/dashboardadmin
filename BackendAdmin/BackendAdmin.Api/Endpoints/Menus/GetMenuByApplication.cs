using BackendAdmin.Application.Features.Menus.DTOs;
using BackendAdmin.Application.Features.Menus.Queries.GetMenuByApplication;

namespace BackendAdmin.Api.Endpoints.Menus;


public record GetMenuByApplicationResponse(List<MenuInfoDTO> Menus);

public class GetMenuByApplication : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/menu/{appAdminReference}", async (string appAdminReference, ISender sender) =>
        {
            var result = await sender.Send(new GetMenuByApplicationQuery(appAdminReference));

            var response = result.Adapt<GetMenuByApplicationResponse>();
            var baseResponse = ResponseFactory.Success(response, "Liste des menus réccuperées avec succès", StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
       .WithName("GetMenuByApplication")
        .WithTags("Menu")
       .Produces<BaseResponse<GetMenuByApplicationResponse>>(StatusCodes.Status200OK)
       .ProducesProblem(StatusCodes.Status400BadRequest)
       .WithSummary("GetMenuByApplication")
       .WithDescription("GetMenuByApplication")
       .RequireAuthorization()
       .WithOpenApi();
    }
}
