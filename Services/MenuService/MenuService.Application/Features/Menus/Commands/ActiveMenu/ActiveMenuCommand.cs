namespace MenuService.Application.Features.Menus.Commands.ActiveMenu;

public record ActiveMenuCommand(string Reference, string AppAdminReference)
    : ICommand<ActiveMenuResult>;

public record ActiveMenuResult(bool Success);

public class ActiveMenuValidator : AbstractValidator<ActiveMenuCommand>
{
    public ActiveMenuValidator()
    {
        RuleFor(x => x.Reference)
            .NotEmpty().WithMessage("La référence du menu est requise.")
            .MaximumLength(50).WithMessage("La référence du menu ne doit pas dépasser 50 caractères.")
            .Matches("^[a-z]+$").WithMessage("La référence du menu ne doit contenir que des lettres minuscule alphabétiques.");
    }
}