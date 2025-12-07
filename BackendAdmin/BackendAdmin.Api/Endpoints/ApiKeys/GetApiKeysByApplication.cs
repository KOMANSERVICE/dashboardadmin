using BackendAdmin.Application.Features.ApiKeys.DTOs;
using BackendAdmin.Application.Features.ApiKeys.Queries.GetApiKeysByApplication;

namespace BackendAdmin.Api.Endpoints.ApiKeys;

public record GetApiKeysByApplicationResponse(IEnumerable<ApiKeyInfoDTO> ApiKeys);

public class GetApiKeysByApplication : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/apikeys/{applicationId}", async (string applicationId, ISender sender) =>
        {
            var query = new GetApiKeysByApplicationQuery(applicationId);
            var result = await sender.Send(query);
            var response = result.Adapt<GetApiKeysByApplicationResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Liste des API Keys recuperee avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetApiKeysByApplication")
        .WithTags("ApiKeys")
        .Produces<BaseResponse<GetApiKeysByApplicationResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Recupere les API Keys d'une application")
        .WithDescription("Retourne toutes les API Keys associees a une application (sans les cles en clair)")
        .RequireAuthorization()
        .WithOpenApi();
    }
}
