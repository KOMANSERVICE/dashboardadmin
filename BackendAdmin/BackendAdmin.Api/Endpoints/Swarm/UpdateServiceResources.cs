using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Commands.UpdateServiceResources;

namespace BackendAdmin.Api.Endpoints.Swarm;

public class UpdateServiceResources : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/swarm/services/{name}/resources", async (string name, UpdateServiceResourcesRequest request, ISender sender) =>
        {
            var command = new UpdateServiceResourcesCommand(
                ServiceName: name,
                CpuLimit: request.CpuLimit,
                CpuReservation: request.CpuReservation,
                MemoryLimit: request.MemoryLimit,
                MemoryReservation: request.MemoryReservation,
                PidsLimit: request.PidsLimit,
                BlkioWeight: request.BlkioWeight,
                Ulimits: request.Ulimits
            );
            var result = await sender.Send(command);
            var response = new UpdateServiceResourcesResponse(result.ServiceName, result.Message);
            var baseResponse = ResponseFactory.Success(
                response,
                result.Message,
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("UpdateServiceResources")
        .WithTags("Swarm")
        .Produces<BaseResponse<UpdateServiceResourcesResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Met a jour les ressources et limites d'un service Docker Swarm")
        .WithDescription("Configure les limites CPU, memoire, PIDs et ulimits pour un service. Les valeurs sont persistees en base de donnees.")
        .RequireAuthorization();
    }
}
