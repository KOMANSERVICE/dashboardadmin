namespace BackendAdmin.Application.Features.Swarm.Commands.DeployStack;

public class DeployStackValidator : AbstractValidator<DeployStackCommand>
{
    public DeployStackValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Le nom de la stack est requis")
            .Matches("^[a-zA-Z0-9][a-zA-Z0-9_.-]*$")
            .WithMessage("Le nom de la stack doit commencer par une lettre ou un chiffre et ne contenir que des lettres, chiffres, tirets, underscores et points");

        RuleFor(x => x.ComposeFileContent)
            .NotEmpty()
            .WithMessage("Le contenu du fichier compose est requis");
    }
}
