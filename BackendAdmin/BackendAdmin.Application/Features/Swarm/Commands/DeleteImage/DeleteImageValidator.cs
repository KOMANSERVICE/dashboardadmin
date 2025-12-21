namespace BackendAdmin.Application.Features.Swarm.Commands.DeleteImage;

public class DeleteImageValidator : AbstractValidator<DeleteImageCommand>
{
    public DeleteImageValidator()
    {
        RuleFor(x => x.ImageId)
            .NotEmpty()
            .WithMessage("L'identifiant de l'image est requis");
    }
}
