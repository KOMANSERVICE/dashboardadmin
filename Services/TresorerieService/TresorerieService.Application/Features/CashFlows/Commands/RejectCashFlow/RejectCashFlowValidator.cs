namespace TresorerieService.Application.Features.CashFlows.Commands.RejectCashFlow;

public class RejectCashFlowValidator : AbstractValidator<RejectCashFlowCommand>
{
    public RejectCashFlowValidator()
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

        RuleFor(x => x.RejectionReason)
            .NotEmpty()
            .WithMessage("Le motif de rejet est obligatoire")
            .MaximumLength(500)
            .WithMessage("Le motif de rejet ne peut pas depasser 500 caracteres");

        RuleFor(x => x.RejectedBy)
            .NotEmpty()
            .WithMessage("L'identifiant du validateur est obligatoire");

        RuleFor(x => x.UserRole)
            .NotEmpty()
            .WithMessage("Le role de l'utilisateur est obligatoire")
            .Must(role => role != null && (role.ToLower() == "manager" || role.ToLower() == "admin"))
            .WithMessage("Acces refuse: seul un manager ou admin peut rejeter un flux");
    }
}
