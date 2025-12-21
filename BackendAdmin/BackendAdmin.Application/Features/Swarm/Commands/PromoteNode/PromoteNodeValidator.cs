namespace BackendAdmin.Application.Features.Swarm.Commands.PromoteNode;

public class PromoteNodeValidator : AbstractValidator<PromoteNodeCommand>
{
    public PromoteNodeValidator()
    {
        RuleFor(x => x.NodeId)
            .NotEmpty()
            .WithMessage("L'identifiant du noeud est requis");
    }
}
