namespace BackendAdmin.Application.Features.Swarm.Commands.UpdateService;

public class UpdateServiceValidator : AbstractValidator<UpdateServiceCommand>
{
    public UpdateServiceValidator()
    {
        RuleFor(x => x.ServiceName)
            .NotEmpty()
            .WithMessage("Le nom du service est requis");

        RuleFor(x => x.Replicas)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Replicas.HasValue)
            .WithMessage("Le nombre de replicas doit etre positif ou zero");

        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.Image) || x.Replicas.HasValue || x.Env != null || x.Labels != null)
            .WithMessage("Au moins un champ doit etre modifie (image, replicas, env ou labels)");
    }
}
