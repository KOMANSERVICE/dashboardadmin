using BackendAdmin.Application.Features.ApiKeys.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.ApiKeys.Commands.RotateApiKey;

public class RotateApiKeyHandler(
    IApiKeyService _apiKeyService
) : ICommandHandler<RotateApiKeyCommand, RotateApiKeyResult>
{
    public async Task<RotateApiKeyResult> Handle(RotateApiKeyCommand request, CancellationToken cancellationToken)
    {
        var dto = request.RotateRequest;

        var response = await _apiKeyService.RotateApiKeyAsync(dto.CurrentApiKeyHash, dto.GracePeriodDays);

        var resultDto = new ApiKeyCreatedDTO
        {
            ApiKey = response.ApiKey,
            ApiKeyHash = response.ApiKeyHash,
            ApplicationId = response.ApplicationId,
            ApplicationName = response.ApplicationName,
            Scopes = response.Scopes,
            CreatedAt = response.CreatedAt,
            ExpiresAt = response.ExpiresAt
        };

        return new RotateApiKeyResult(resultDto);
    }
}
