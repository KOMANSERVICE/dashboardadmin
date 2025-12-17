namespace TresorerieService.Application.Features.RecurringCashFlows.Commands.ToggleRecurringCashFlow;

public class ToggleRecurringCashFlowValidator : AbstractValidator<ToggleRecurringCashFlowCommand>
{
    public ToggleRecurringCashFlowValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("L'identifiant du flux recurrent est obligatoire");

        RuleFor(x => x.ApplicationId)
            .NotEmpty()
            .WithMessage("L'identifiant de l'application est obligatoire");

        RuleFor(x => x.BoutiqueId)
            .NotEmpty()
            .WithMessage("L'identifiant de la boutique est obligatoire");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("L'identifiant de l'utilisateur est obligatoire");
    }
}
