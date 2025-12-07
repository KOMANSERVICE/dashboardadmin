using BackendAdmin.Application.Features.ApiKeys.DTOs;

namespace BackendAdmin.Application.Features.ApiKeys.Commands.RevokeApiKey;

public record RevokeApiKeyCommand(RevokeApiKeyDTO RevokeRequest)
    : ICommand<RevokeApiKeyResult>;

public record RevokeApiKeyResult(bool Success);

public class RevokeApiKeyValidator : AbstractValidator<RevokeApiKeyCommand>
{
    public RevokeApiKeyValidator()
    {
        RuleFor(x => x.RevokeRequest).NotNull().WithMessage("Les informations de revocation sont requises.");

        RuleFor(x => x.RevokeRequest.ApiKeyHash)
            .NotEmpty().WithMessage("Le hash de l'API Key est requis.");

        RuleFor(x => x.RevokeRequest.Reason)
            .NotEmpty().WithMessage("La raison de la revocation est requise.")
            .MaximumLength(500).WithMessage("La raison ne doit pas depasser 500 caracteres.");
    }
}
