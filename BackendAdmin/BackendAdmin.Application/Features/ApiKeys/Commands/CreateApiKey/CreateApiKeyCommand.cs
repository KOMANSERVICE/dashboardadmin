using BackendAdmin.Application.Features.ApiKeys.DTOs;

namespace BackendAdmin.Application.Features.ApiKeys.Commands.CreateApiKey;

public record CreateApiKeyCommand(CreateApiKeyDTO ApiKey)
    : ICommand<CreateApiKeyResult>;

public record CreateApiKeyResult(ApiKeyCreatedDTO ApiKey);

public class CreateApiKeyValidator : AbstractValidator<CreateApiKeyCommand>
{
    public CreateApiKeyValidator()
    {
        RuleFor(x => x.ApiKey).NotNull().WithMessage("Les informations de l'API Key sont requises.");

        RuleFor(x => x.ApiKey.ApplicationId)
            .NotEmpty().WithMessage("L'identifiant de l'application est requis.")
            .MaximumLength(100).WithMessage("L'identifiant de l'application ne doit pas depasser 100 caracteres.");

        RuleFor(x => x.ApiKey.ApplicationName)
            .NotEmpty().WithMessage("Le nom de l'application est requis.")
            .MaximumLength(200).WithMessage("Le nom de l'application ne doit pas depasser 200 caracteres.");

        RuleFor(x => x.ApiKey.Scopes)
            .NotNull().WithMessage("Les scopes sont requis.")
            .NotEmpty().WithMessage("Au moins un scope est requis.")
            .Must(scopes => scopes.All(s => !string.IsNullOrWhiteSpace(s)))
            .WithMessage("Les scopes ne peuvent pas etre vides.");
    }
}
