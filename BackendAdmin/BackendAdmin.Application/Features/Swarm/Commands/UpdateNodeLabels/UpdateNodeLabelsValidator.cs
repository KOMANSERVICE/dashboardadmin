namespace BackendAdmin.Application.Features.Swarm.Commands.UpdateNodeLabels;

public class UpdateNodeLabelsValidator : AbstractValidator<UpdateNodeLabelsCommand>
{
    public UpdateNodeLabelsValidator()
    {
        RuleFor(x => x.NodeId)
            .NotEmpty()
            .WithMessage("L'identifiant du noeud est requis");

        RuleFor(x => x.Labels)
            .NotNull()
            .WithMessage("Les labels sont requis");
    }
}
