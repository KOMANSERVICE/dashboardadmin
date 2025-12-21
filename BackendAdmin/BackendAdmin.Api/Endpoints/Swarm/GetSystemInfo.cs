using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Queries.GetSystemInfo;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record GetSystemInfoResponse(SystemInfoDTO SystemInfo);

public class GetSystemInfo : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/swarm/system/info", async (ISender sender) =>
        {
            var query = new GetSystemInfoQuery();
            var result = await sender.Send(query);
            var response = result.Adapt<GetSystemInfoResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Informations systeme Docker recuperees avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetSystemInfo")
        .WithTags("Swarm", "System")
        .Produces<BaseResponse<GetSystemInfoResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Recupere les informations systeme Docker")
        .WithDescription("Retourne les informations detaillees du systeme Docker: OS, architecture, ressources, nombre de conteneurs/images, etc.")
        .RequireAuthorization();
    }
}
