namespace BackendAdmin.Application.Features.Swarm.Commands.RestoreVolume;

public class RestoreVolumeValidator : AbstractValidator<RestoreVolumeCommand>
{
    public RestoreVolumeValidator()
    {
        RuleFor(x => x.VolumeName)
            .NotEmpty()
            .WithMessage("Le nom du volume est requis")
            .Matches("^[a-zA-Z0-9][a-zA-Z0-9_.-]*$")
            .WithMessage("Le nom du volume doit commencer par une lettre ou un chiffre et ne contenir que des lettres, chiffres, tirets, underscores et points");

        RuleFor(x => x.SourcePath)
            .NotEmpty()
            .WithMessage("Le chemin source est requis")
            .Must(path => !path.Contains(".."))
            .WithMessage("Le chemin source ne peut pas contenir '..' (path traversal)");
    }
}
