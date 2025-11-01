using BackendAdmin.Application.UseCases.AppAdmins.Commands.CreateAppAdmin;
using BackendAdmin.Application.UseCases.AppAdmins.DTOs;

namespace BackendAdmin.Api.Endpoints.AppAdmins;

public record CreateAppAdminRequest(AppAdminDTO AppAdmin);
public record CreateAppAdminResponse(Guid Id);
public class CreateAppAdmin : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/application", async (CreateAppAdminRequest request, ISender sender) =>
        {
            var query = request.Adapt<CreateAppAdminCommand>();

            var result = await sender.Send(query);

            var response = result.Adapt<CreateAppAdminResponse>();
            var baseResponse = ResponseFactory.Success(response, "Application créer avec succèss", StatusCodes.Status201Created);

            return Results.Created($"/application", baseResponse);
        })
       .WithName("CreateAppAdmin")
       .WithTags("Application")
       .Produces<BaseResponse<CreateAppAdminResponse>>(StatusCodes.Status201Created)
       .ProducesProblem(StatusCodes.Status400BadRequest)
       .WithSummary("CreateAppAdmin")
       .WithDescription("CreateAppAdmin")
       .RequireAuthorization()
       .WithOpenApi();
    }
}
