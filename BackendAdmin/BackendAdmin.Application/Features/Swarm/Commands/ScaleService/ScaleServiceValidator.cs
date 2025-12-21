namespace BackendAdmin.Application.Features.Swarm.Commands.ScaleService;

public class ScaleServiceValidator : AbstractValidator<ScaleServiceCommand>
{
    public ScaleServiceValidator()
    {
        RuleFor(x => x.ServiceName)
            .NotEmpty()
            .WithMessage("Le nom du service est requis");

        RuleFor(x => x.Replicas)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Le nombre de replicas doit etre >= 0");
    }
}
