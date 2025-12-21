namespace BackendAdmin.Application.Features.Swarm.Commands.DeleteService;

public class DeleteServiceValidator : AbstractValidator<DeleteServiceCommand>
{
    public DeleteServiceValidator()
    {
        RuleFor(x => x.ServiceName)
            .NotEmpty()
            .WithMessage("Le nom du service est requis");
    }
}
