using BackendAdmin.Application.Features.ApiKeys.Commands.CreateApiKey;
using BackendAdmin.Application.Features.ApiKeys.DTOs;

namespace BackendAdmin.Api.Endpoints.ApiKeys;

public record CreateApiKeyRequest(CreateApiKeyDTO ApiKey);
public record CreateApiKeyResponse(ApiKeyCreatedDTO ApiKey);

public class CreateApiKey : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/apikeys", async (CreateApiKeyRequest request, ISender sender) =>
        {
            var command = request.Adapt<CreateApiKeyCommand>();
            var result = await sender.Send(command);
            var response = result.Adapt<CreateApiKeyResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "API Key creee avec succes. ATTENTION: La cle en clair ne sera plus jamais affichee.",
                StatusCodes.Status201Created);

            return Results.Created("/apikeys", baseResponse);
        })
        .WithName("CreateApiKey")
        .WithTags("ApiKeys")
        .Produces<BaseResponse<CreateApiKeyResponse>>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Cree une nouvelle API Key")
        .WithDescription("Genere une nouvelle API Key pour une application. La cle en clair n'est retournee qu'une seule fois.")
        .RequireAuthorization()
        .WithOpenApi();
    }
}
