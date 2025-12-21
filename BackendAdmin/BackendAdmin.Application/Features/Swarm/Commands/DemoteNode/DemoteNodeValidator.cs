namespace BackendAdmin.Application.Features.Swarm.Commands.DemoteNode;

public class DemoteNodeValidator : AbstractValidator<DemoteNodeCommand>
{
    public DemoteNodeValidator()
    {
        RuleFor(x => x.NodeId)
            .NotEmpty()
            .WithMessage("L'identifiant du noeud est requis");
    }
}
