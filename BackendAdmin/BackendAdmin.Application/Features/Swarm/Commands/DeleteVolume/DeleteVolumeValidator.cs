namespace BackendAdmin.Application.Features.Swarm.Commands.DeleteVolume;

public class DeleteVolumeValidator : AbstractValidator<DeleteVolumeCommand>
{
    public DeleteVolumeValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Le nom du volume est requis");
    }
}
