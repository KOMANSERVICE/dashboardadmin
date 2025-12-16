namespace TresorerieService.Application.Features.CashFlows.Commands.SubmitCashFlow;

public class SubmitCashFlowValidator : AbstractValidator<SubmitCashFlowCommand>
{
    public SubmitCashFlowValidator()
    {
        RuleFor(x => x.CashFlowId)
            .NotEmpty()
            .WithMessage("L'identifiant du flux de tresorerie est obligatoire");

        RuleFor(x => x.ApplicationId)
            .NotEmpty()
            .WithMessage("L'identifiant de l'application est obligatoire");

        RuleFor(x => x.BoutiqueId)
            .NotEmpty()
            .WithMessage("L'identifiant de la boutique est obligatoire");

        RuleFor(x => x.SubmittedBy)
            .NotEmpty()
            .WithMessage("L'identifiant de l'utilisateur qui soumet est obligatoire");
    }
}
