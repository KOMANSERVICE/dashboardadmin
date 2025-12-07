using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.ApiKeys.Commands.RevokeApiKey;

public class RevokeApiKeyHandler(
    IApiKeyService _apiKeyService
) : ICommandHandler<RevokeApiKeyCommand, RevokeApiKeyResult>
{
    public async Task<RevokeApiKeyResult> Handle(RevokeApiKeyCommand request, CancellationToken cancellationToken)
    {
        var dto = request.RevokeRequest;

        await _apiKeyService.RevokeApiKeyAsync(dto.ApiKeyHash, dto.Reason);

        return new RevokeApiKeyResult(true);
    }
}
