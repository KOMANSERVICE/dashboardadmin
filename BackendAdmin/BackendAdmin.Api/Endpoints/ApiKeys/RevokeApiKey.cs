using BackendAdmin.Application.Features.ApiKeys.Commands.RevokeApiKey;
using BackendAdmin.Application.Features.ApiKeys.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BackendAdmin.Api.Endpoints.ApiKeys;

public record RevokeApiKeyResponse(bool Success);

public class RevokeApiKey : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/apikeys", async ([FromBody] RevokeApiKeyDTO request, ISender sender) =>
        {
            var command = new RevokeApiKeyCommand(request);
            var result = await sender.Send(command);
            var response = result.Adapt<RevokeApiKeyResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "API Key revoquee avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("RevokeApiKey")
        .WithTags("ApiKeys")
        .Produces<BaseResponse<RevokeApiKeyResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Revoque une API Key")
        .WithDescription("Revoque definitivement une API Key. L'operation est irreversible.")
        .RequireAuthorization()
        .WithOpenApi();
    }
}
