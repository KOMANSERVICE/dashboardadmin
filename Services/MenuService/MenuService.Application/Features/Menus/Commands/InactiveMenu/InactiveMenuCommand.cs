namespace MenuService.Application.Features.Menus.Commands.InactiveMenu;

public record InactiveMenuCommand(string Reference, string AppAdminReference)
    : ICommand<InactiveMenuResult>;

public record InactiveMenuResult(bool Success);

public class InactiveMenuValidator : AbstractValidator<InactiveMenuCommand>
{
    public InactiveMenuValidator()
    {
        RuleFor(x => x.Reference)
            .NotEmpty().WithMessage("La référence du menu est requise.")
            .MaximumLength(50).WithMessage("La référence du menu ne doit pas dépasser 50 caractères.")
            .Matches("^[a-z]+$").WithMessage("La référence du menu ne doit contenir que des lettres minuscule alphabétiques.");
    }
}
