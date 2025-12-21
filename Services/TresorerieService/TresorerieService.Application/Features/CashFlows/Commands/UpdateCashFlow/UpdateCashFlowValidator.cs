namespace TresorerieService.Application.Features.CashFlows.Commands.UpdateCashFlow;

public class UpdateCashFlowValidator : AbstractValidator<UpdateCashFlowCommand>
{
    public UpdateCashFlowValidator()
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

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("L'identifiant de l'utilisateur est obligatoire");

        RuleFor(x => x.Data)
            .NotNull()
            .WithMessage("Les donnees de mise a jour sont obligatoires");

        When(x => x.Data != null, () =>
        {
            RuleFor(x => x.Data.Label)
                .MaximumLength(200)
                .WithMessage("Le libelle ne peut pas depasser 200 caracteres")
                .When(x => x.Data.Label != null);

            RuleFor(x => x.Data.Description)
                .MaximumLength(1000)
                .WithMessage("La description ne peut pas depasser 1000 caracteres")
                .When(x => x.Data.Description != null);

            RuleFor(x => x.Data.Amount)
                .GreaterThan(0)
                .WithMessage("Le montant doit etre superieur a 0")
                .When(x => x.Data.Amount.HasValue);

            RuleFor(x => x.Data.TaxAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Le montant de la taxe doit etre superieur ou egal a 0")
                .When(x => x.Data.TaxAmount.HasValue);

            RuleFor(x => x.Data.TaxRate)
                .InclusiveBetween(0, 100)
                .WithMessage("Le taux de taxe doit etre compris entre 0 et 100")
                .When(x => x.Data.TaxRate.HasValue);

            RuleFor(x => x.Data.Currency)
                .MaximumLength(10)
                .WithMessage("La devise ne peut pas depasser 10 caracteres")
                .When(x => x.Data.Currency != null);

            RuleFor(x => x.Data.PaymentMethod)
                .MaximumLength(50)
                .WithMessage("Le mode de paiement ne peut pas depasser 50 caracteres")
                .When(x => x.Data.PaymentMethod != null);

            RuleFor(x => x.Data.ThirdPartyName)
                .MaximumLength(200)
                .WithMessage("Le nom du tiers ne peut pas depasser 200 caracteres")
                .When(x => x.Data.ThirdPartyName != null);

            RuleFor(x => x.Data.ThirdPartyId)
                .MaximumLength(100)
                .WithMessage("L'identifiant du tiers ne peut pas depasser 100 caracteres")
                .When(x => x.Data.ThirdPartyId != null);

            RuleFor(x => x.Data.AttachmentUrl)
                .MaximumLength(500)
                .WithMessage("L'URL de la piece jointe ne peut pas depasser 500 caracteres")
                .When(x => x.Data.AttachmentUrl != null);
        });
    }
}
