namespace MenuService.Application.Features.Menus.Commands.CreateMenu;

public record CreateMenuCommand(MenuDTO Menu)
    : ICommand<CreateMenuResult>;

public record CreateMenuResult(Guid Id);

public class CreateMenuValidator : AbstractValidator<CreateMenuCommand>
{
    public CreateMenuValidator()
    {
        RuleFor(x => x.Menu).NotNull().WithMessage("Le menu est requis.");
        RuleFor(x => x.Menu.Name)
            .NotEmpty().WithMessage("Le nom du menu est requis.")
            .MaximumLength(100).WithMessage("Le nom du menu ne doit pas dépasser 100 caractères.");
        RuleFor(x => x.Menu.Reference)
            .NotEmpty().WithMessage("La référence du menu est requise.")
            .MaximumLength(50).WithMessage("La référence du menu ne doit pas dépasser 50 caractères.")
            .Matches("^[a-z]+$").WithMessage("La référence du menu ne doit contenir que des lettres minuscule alphabétiques.");
        RuleFor(x => x.Menu.UrlFront)
            .NotEmpty().WithMessage("L'URL front du menu est requise.")
            .MaximumLength(200).WithMessage("L'URL front du menu ne doit pas dépasser 200 caractères.");
        RuleFor(x => x.Menu.Icon)
            .MaximumLength(100).WithMessage("L'icône du menu ne doit pas dépasser 100 caractères.");
        RuleFor(x => x.Menu.AppAdminReference)
            .NotEmpty().WithMessage("La référence de l'application admin est requise.")
            .MaximumLength(50).WithMessage("La référence de l'application admin ne doit pas dépasser 50 caractères.");
    }
}
