namespace BackendAdmin.Application.Features.Swarm.Commands.CreateVolume;

public class CreateVolumeValidator : AbstractValidator<CreateVolumeCommand>
{
    public CreateVolumeValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Le nom du volume est requis")
            .Matches("^[a-zA-Z0-9][a-zA-Z0-9_.-]*$")
            .WithMessage("Le nom du volume doit commencer par une lettre ou un chiffre et ne contenir que des lettres, chiffres, tirets, underscores et points");

        RuleFor(x => x.Driver)
            .NotEmpty()
            .WithMessage("Le driver est requis");
    }
}
