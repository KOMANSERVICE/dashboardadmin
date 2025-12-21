namespace BackendAdmin.Application.Features.Swarm.Commands.RemoveNode;

public class RemoveNodeValidator : AbstractValidator<RemoveNodeCommand>
{
    public RemoveNodeValidator()
    {
        RuleFor(x => x.NodeId)
            .NotEmpty()
            .WithMessage("L'identifiant du noeud est requis");
    }
}
