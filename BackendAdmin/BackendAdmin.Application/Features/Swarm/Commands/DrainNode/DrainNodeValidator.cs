namespace BackendAdmin.Application.Features.Swarm.Commands.DrainNode;

public class DrainNodeValidator : AbstractValidator<DrainNodeCommand>
{
    public DrainNodeValidator()
    {
        RuleFor(x => x.NodeId)
            .NotEmpty()
            .WithMessage("L'identifiant du noeud est requis");
    }
}
