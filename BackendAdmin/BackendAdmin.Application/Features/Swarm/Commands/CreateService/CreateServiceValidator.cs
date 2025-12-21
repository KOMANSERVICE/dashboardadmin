namespace BackendAdmin.Application.Features.Swarm.Commands.CreateService;

public class CreateServiceValidator : AbstractValidator<CreateServiceCommand>
{
    public CreateServiceValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Le nom du service est requis")
            .Matches("^[a-zA-Z0-9][a-zA-Z0-9_.-]*$")
            .WithMessage("Le nom du service doit commencer par une lettre ou un chiffre et ne contenir que des lettres, chiffres, tirets, underscores et points");

        RuleFor(x => x.Image)
            .NotEmpty()
            .WithMessage("L'image Docker est requise");

        RuleFor(x => x.Replicas)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Le nombre de replicas doit etre positif ou zero");
    }
}
