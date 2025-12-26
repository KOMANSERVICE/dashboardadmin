namespace TresorerieService.Application.Features.CashFlows.Commands.ReverseCashFlow;

public class ReverseCashFlowValidator : AbstractValidator<ReverseCashFlowCommand>
{
    public ReverseCashFlowValidator()
    {
        RuleFor(x => x.CashFlowId)
            .NotEmpty()
            .WithMessage("L'ID du CashFlow est obligatoire");

        RuleFor(x => x.ApplicationId)
            .NotEmpty()
            .WithMessage("L'ApplicationId est obligatoire");

        RuleFor(x => x.BoutiqueId)
            .NotEmpty()
            .WithMessage("Le BoutiqueId est obligatoire");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Le motif de l'annulation est obligatoire")
            .MaximumLength(500)
            .WithMessage("Le motif ne peut pas depasser 500 caracteres");
    }
}
