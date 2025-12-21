namespace BackendAdmin.Application.Features.Swarm.Commands.RestartService;

public class RestartServiceValidator : AbstractValidator<RestartServiceCommand>
{
    public RestartServiceValidator()
    {
        RuleFor(x => x.ServiceName)
            .NotEmpty()
            .WithMessage("Le nom du service est requis");
    }
}
