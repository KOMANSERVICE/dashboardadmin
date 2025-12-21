namespace TresorerieService.Application.Features.CashFlows.Commands.ApproveCashFlow;

public class ApproveCashFlowValidator : AbstractValidator<ApproveCashFlowCommand>
{
    public ApproveCashFlowValidator()
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

        RuleFor(x => x.ValidatedBy)
            .NotEmpty()
            .WithMessage("L'identifiant du validateur est obligatoire");

        RuleFor(x => x.UserRole)
            .NotEmpty()
            .WithMessage("Le role de l'utilisateur est obligatoire")
            .Must(role => role != null && (role.ToLower() == "manager" || role.ToLower() == "admin"))
            .WithMessage("Acces refuse: seul un manager ou admin peut valider un flux");
    }
}
