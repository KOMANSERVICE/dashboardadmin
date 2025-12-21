namespace TresorerieService.Application.Features.CashFlows.Commands.CreateTransfer;

public class CreateTransferValidator : AbstractValidator<CreateTransferCommand>
{
    public CreateTransferValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty()
            .WithMessage("L'en-tete X-Application-Id est obligatoire");

        RuleFor(x => x.BoutiqueId)
            .NotEmpty()
            .WithMessage("L'en-tete X-Boutique-Id est obligatoire");

        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("L'identifiant du compte source est obligatoire");

        RuleFor(x => x.DestinationAccountId)
            .NotEmpty()
            .WithMessage("L'identifiant du compte destination est obligatoire");

        RuleFor(x => x.AccountId)
            .NotEqual(x => x.DestinationAccountId)
            .WithMessage("Le compte source et le compte destination doivent etre differents");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Le montant doit etre superieur a 0");

        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("La date est obligatoire");

        RuleFor(x => x.Label)
            .NotEmpty()
            .WithMessage("Le libelle est obligatoire")
            .MaximumLength(200)
            .WithMessage("Le libelle ne peut pas depasser 200 caracteres");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("La description ne peut pas depasser 1000 caracteres")
            .When(x => x.Description != null);

        RuleFor(x => x.CreatedBy)
            .NotEmpty()
            .WithMessage("L'identifiant du createur est obligatoire");
    }
}
