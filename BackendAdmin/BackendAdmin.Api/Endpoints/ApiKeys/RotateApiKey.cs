using BackendAdmin.Application.Features.ApiKeys.Commands.RotateApiKey;
using BackendAdmin.Application.Features.ApiKeys.DTOs;

namespace BackendAdmin.Api.Endpoints.ApiKeys;

public record RotateApiKeyResponse(ApiKeyCreatedDTO NewApiKey);

public class RotateApiKey : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/apikeys/rotate", async (RotateApiKeyDTO request, ISender sender) =>
        {
            var command = new RotateApiKeyCommand(request);
            var result = await sender.Send(command);
            var response = result.Adapt<RotateApiKeyResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "API Key rotee avec succes. ATTENTION: La nouvelle cle en clair ne sera plus jamais affichee.",
                StatusCodes.Status201Created);

            return Results.Created("/apikeys", baseResponse);
        })
        .WithName("RotateApiKey")
        .WithTags("ApiKeys")
        .Produces<BaseResponse<RotateApiKeyResponse>>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Effectue une rotation d'API Key")
        .WithDescription("Cree une nouvelle API Key et revoque l'ancienne apres une periode de grace configurable")
        .RequireAuthorization()
        .WithOpenApi();
    }
}
