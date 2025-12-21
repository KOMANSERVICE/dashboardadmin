using FluentValidation;

namespace BackendAdmin.Application.Features.Swarm.Commands.ExecContainer;

public class ExecContainerValidator : AbstractValidator<ExecContainerCommand>
{
    public ExecContainerValidator()
    {
        RuleFor(x => x.ContainerId)
            .NotEmpty()
            .WithMessage("L'identifiant du conteneur est requis");

        RuleFor(x => x.Command)
            .NotEmpty()
            .WithMessage("La commande est requise");
    }
}
