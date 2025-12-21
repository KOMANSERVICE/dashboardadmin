namespace TresorerieService.Application.Features.CashFlows.Commands.ReconcileCashFlows;

public class ReconcileCashFlowsValidator : AbstractValidator<ReconcileCashFlowsCommand>
{
    public ReconcileCashFlowsValidator()
    {
        RuleFor(x => x.CashFlowIds)
            .NotNull()
            .WithMessage("La liste des identifiants de flux est obligatoire")
            .NotEmpty()
            .WithMessage("Au moins un flux est requis")
            .Must(ids => ids.All(id => id != Guid.Empty))
            .WithMessage("Tous les identifiants de flux doivent etre valides");

        RuleFor(x => x.ApplicationId)
            .NotEmpty()
            .WithMessage("L'identifiant de l'application est obligatoire");

        RuleFor(x => x.BoutiqueId)
            .NotEmpty()
            .WithMessage("L'identifiant de la boutique est obligatoire");

        RuleFor(x => x.ReconciledBy)
            .NotEmpty()
            .WithMessage("L'identifiant de l'utilisateur effectuant la reconciliation est obligatoire");

        RuleFor(x => x.UserRole)
            .NotEmpty()
            .WithMessage("Le role de l'utilisateur est obligatoire")
            .Must(role => role != null && (role.ToLower() == "manager" || role.ToLower() == "admin"))
            .WithMessage("Acces refuse: seul un manager ou admin peut reconcilier des flux");

        RuleFor(x => x.BankStatementReference)
            .MaximumLength(100)
            .WithMessage("La reference du releve bancaire ne peut pas depasser 100 caracteres");
    }
}
