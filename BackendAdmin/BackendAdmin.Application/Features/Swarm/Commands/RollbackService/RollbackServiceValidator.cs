namespace BackendAdmin.Application.Features.Swarm.Commands.RollbackService;

public class RollbackServiceValidator : AbstractValidator<RollbackServiceCommand>
{
    public RollbackServiceValidator()
    {
        RuleFor(x => x.ServiceName)
            .NotEmpty()
            .WithMessage("Le nom du service est requis");
    }
}
