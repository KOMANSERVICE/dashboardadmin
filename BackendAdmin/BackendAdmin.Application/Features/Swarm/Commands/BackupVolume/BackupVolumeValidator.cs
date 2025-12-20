namespace BackendAdmin.Application.Features.Swarm.Commands.BackupVolume;

public class BackupVolumeValidator : AbstractValidator<BackupVolumeCommand>
{
    public BackupVolumeValidator()
    {
        RuleFor(x => x.VolumeName)
            .NotEmpty()
            .WithMessage("Le nom du volume est requis");

        RuleFor(x => x.DestinationPath)
            .NotEmpty()
            .WithMessage("Le chemin de destination est requis")
            .Must(path => !path.Contains(".."))
            .WithMessage("Le chemin de destination ne peut pas contenir '..' (path traversal)");
    }
}
