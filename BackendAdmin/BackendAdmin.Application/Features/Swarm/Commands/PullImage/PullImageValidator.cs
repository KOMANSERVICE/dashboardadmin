namespace BackendAdmin.Application.Features.Swarm.Commands.PullImage;

public class PullImageValidator : AbstractValidator<PullImageCommand>
{
    public PullImageValidator()
    {
        RuleFor(x => x.Image)
            .NotEmpty()
            .WithMessage("Le nom de l'image est requis");

        RuleFor(x => x.Tag)
            .MaximumLength(128)
            .When(x => !string.IsNullOrEmpty(x.Tag))
            .WithMessage("Le tag ne peut pas depasser 128 caracteres");
    }
}
