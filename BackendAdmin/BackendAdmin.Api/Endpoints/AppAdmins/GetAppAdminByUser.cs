using BackendAdmin.Application.UseCases.AppAdmins.DTOs;
using BackendAdmin.Application.UseCases.AppAdmins.Queries.GetAppAdminByUser;

namespace BackendAdmin.Api.Endpoints.AppAdmins;
public record GetAppAdminByUserResponse(IEnumerable<AppAdminDTO> AppAdmins);

public class GetAppAdminByUser : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/application", async (ISender sender) =>
        {
            var result = await sender.Send(new GetAppAdminByUserQuery());

            var response = result.Adapt<GetAppAdminByUserResponse>();
            var baseResponse = ResponseFactory.Success(response, "Liste des applications réccuperées avec succès", StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
       .WithName("GetAppAdminByUser")
        .WithTags("Application")
       .Produces<BaseResponse<GetAppAdminByUserResponse>>(StatusCodes.Status200OK)
       .ProducesProblem(StatusCodes.Status400BadRequest)
       .WithSummary("GetAppAdminByUser")
       .WithDescription("GetAppAdminByUser")
       .RequireAuthorization()
       .WithOpenApi();
    }
}

