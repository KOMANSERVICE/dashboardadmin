namespace BackendAdmin.Application.Features.Swarm.Commands.TagImage;

public class TagImageValidator : AbstractValidator<TagImageCommand>
{
    public TagImageValidator()
    {
        RuleFor(x => x.ImageId)
            .NotEmpty()
            .WithMessage("L'identifiant de l'image est requis");

        RuleFor(x => x.NewRepository)
            .NotEmpty()
            .WithMessage("Le nouveau repository est requis")
            .MaximumLength(256)
            .WithMessage("Le repository ne peut pas depasser 256 caracteres");

        RuleFor(x => x.NewTag)
            .NotEmpty()
            .WithMessage("Le nouveau tag est requis")
            .MaximumLength(128)
            .WithMessage("Le tag ne peut pas depasser 128 caracteres");
    }
}
