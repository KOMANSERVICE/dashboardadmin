using BackendAdmin.Application.Features.ApiKeys.DTOs;

namespace BackendAdmin.Application.Features.ApiKeys.Commands.RotateApiKey;

public record RotateApiKeyCommand(RotateApiKeyDTO RotateRequest)
    : ICommand<RotateApiKeyResult>;

public record RotateApiKeyResult(ApiKeyCreatedDTO NewApiKey);

public class RotateApiKeyValidator : AbstractValidator<RotateApiKeyCommand>
{
    public RotateApiKeyValidator()
    {
        RuleFor(x => x.RotateRequest).NotNull().WithMessage("Les informations de rotation sont requises.");

        RuleFor(x => x.RotateRequest.CurrentApiKeyHash)
            .NotEmpty().WithMessage("Le hash de l'API Key actuelle est requis.");

        RuleFor(x => x.RotateRequest.GracePeriodDays)
            .GreaterThanOrEqualTo(0).WithMessage("La periode de grace doit etre positive.")
            .LessThanOrEqualTo(30).WithMessage("La periode de grace ne doit pas depasser 30 jours.");
    }
}
