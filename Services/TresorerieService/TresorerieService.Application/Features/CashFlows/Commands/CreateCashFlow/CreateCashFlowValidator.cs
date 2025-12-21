namespace TresorerieService.Application.Features.CashFlows.Commands.CreateCashFlow;

public class CreateCashFlowValidator : AbstractValidator<CreateCashFlowCommand>
{
    public CreateCashFlowValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty()
            .WithMessage("L'identifiant de l'application est obligatoire");

        RuleFor(x => x.BoutiqueId)
            .NotEmpty()
            .WithMessage("L'identifiant de la boutique est obligatoire");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Le type de flux est invalide")
            .Must(t => t == CashFlowType.INCOME || t == CashFlowType.EXPENSE)
            .WithMessage("Le type de flux doit etre INCOME ou EXPENSE");

        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage("L'identifiant de la categorie est obligatoire");

        RuleFor(x => x.Label)
            .NotEmpty()
            .WithMessage("Le libelle est obligatoire")
            .MaximumLength(200)
            .WithMessage("Le libelle ne peut pas depasser 200 caracteres");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("La description ne peut pas depasser 1000 caracteres")
            .When(x => x.Description != null);

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Le montant doit etre superieur a 0");

        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("L'identifiant du compte est obligatoire");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty()
            .WithMessage("Le mode de paiement est obligatoire")
            .MaximumLength(50)
            .WithMessage("Le mode de paiement ne peut pas depasser 50 caracteres");

        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("La date est obligatoire");

        RuleFor(x => x.ThirdPartyName)
            .MaximumLength(200)
            .WithMessage("Le nom du tiers ne peut pas depasser 200 caracteres")
            .When(x => x.ThirdPartyName != null);

        RuleFor(x => x.AttachmentUrl)
            .MaximumLength(500)
            .WithMessage("L'URL de la piece jointe ne peut pas depasser 500 caracteres")
            .When(x => x.AttachmentUrl != null);

        RuleFor(x => x.CreatedBy)
            .NotEmpty()
            .WithMessage("L'identifiant du createur est obligatoire");
    }
}
