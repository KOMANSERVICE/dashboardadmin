namespace BackendAdmin.Application.Features.Swarm.Commands.PushImage;

public class PushImageValidator : AbstractValidator<PushImageCommand>
{
    public PushImageValidator()
    {
        RuleFor(x => x.ImageId)
            .NotEmpty()
            .WithMessage("L'identifiant de l'image est requis");
    }
}
