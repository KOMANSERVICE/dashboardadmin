using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Queries.GetImageHistory;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record GetImageHistoryResponse(List<ImageHistoryDTO> History);

public class GetImageHistory : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/swarm/images/{id}/history", async (string id, ISender sender) =>
        {
            var query = new GetImageHistoryQuery(id);
            var result = await sender.Send(query);
            var response = result.Adapt<GetImageHistoryResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Historique de l'image recupere avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetImageHistory")
        .WithTags("Swarm", "Images")
        .Produces<BaseResponse<GetImageHistoryResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Recupere l'historique d'une image Docker")
        .WithDescription("Retourne l'historique des layers d'une image Docker avec les commandes utilisees pour les creer")
        .RequireAuthorization();
    }
}
