namespace BackendAdmin.Application.Features.Swarm.Commands.DeleteStack;

public class DeleteStackValidator : AbstractValidator<DeleteStackCommand>
{
    public DeleteStackValidator()
    {
        RuleFor(x => x.StackName)
            .NotEmpty()
            .WithMessage("Le nom de la stack est requis");
    }
}
