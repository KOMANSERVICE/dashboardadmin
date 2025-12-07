using BackendAdmin.Application.Features.ApiKeys.DTOs;

namespace BackendAdmin.Application.Features.ApiKeys.Queries.GetApiKeysByApplication;

public record GetApiKeysByApplicationQuery(string ApplicationId)
    : IQuery<GetApiKeysByApplicationResult>;

public record GetApiKeysByApplicationResult(IEnumerable<ApiKeyInfoDTO> ApiKeys);

public class GetApiKeysByApplicationValidator : AbstractValidator<GetApiKeysByApplicationQuery>
{
    public GetApiKeysByApplicationValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty().WithMessage("L'identifiant de l'application est requis.");
    }
}
