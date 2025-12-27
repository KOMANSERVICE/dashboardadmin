namespace TresorerieService.Application.Features.CashFlows.Commands.DeleteCashFlow;

public class DeleteCashFlowValidator : AbstractValidator<DeleteCashFlowCommand>
{
    public DeleteCashFlowValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("L'identifiant du flux est obligatoire");

        RuleFor(x => x.ApplicationId)
            .NotEmpty()
            .WithMessage("L'identifiant de l'application est obligatoire");

        RuleFor(x => x.BoutiqueId)
            .NotEmpty()
            .WithMessage("L'identifiant de la boutique est obligatoire");
    }
}
