using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Queries.GetVolumeDetails;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record GetVolumeDetailsResponse(VolumeDetailsDTO Volume);

public class GetVolumeDetails : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/swarm/volumes/{name}", async (string name, ISender sender) =>
        {
            var query = new GetVolumeDetailsQuery(name);
            var result = await sender.Send(query);
            var response = result.Adapt<GetVolumeDetailsResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                $"Details du volume '{name}' recuperes avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetVolumeDetails")
        .WithTags("Swarm", "Volumes")
        .Produces<BaseResponse<GetVolumeDetailsResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Inspecte un volume Docker")
        .WithDescription("Retourne les details complets d'un volume Docker incluant la taille, le mount point, le driver, les labels et les containers qui l'utilisent")
        .RequireAuthorization();
    }
}
