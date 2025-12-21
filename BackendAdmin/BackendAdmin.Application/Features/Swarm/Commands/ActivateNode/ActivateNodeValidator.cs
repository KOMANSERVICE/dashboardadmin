namespace BackendAdmin.Application.Features.Swarm.Commands.ActivateNode;

public class ActivateNodeValidator : AbstractValidator<ActivateNodeCommand>
{
    public ActivateNodeValidator()
    {
        RuleFor(x => x.NodeId)
            .NotEmpty()
            .WithMessage("L'identifiant du noeud est requis");
    }
}
