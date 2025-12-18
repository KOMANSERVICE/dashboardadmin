namespace TresorerieService.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty()
            .WithMessage("L'identifiant de l'application est obligatoire");

        RuleFor(x => x.BoutiqueId)
            .NotEmpty()
            .WithMessage("L'identifiant de la boutique est obligatoire");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Le nom de la catégorie est obligatoire")
            .MaximumLength(200)
            .WithMessage("Le nom de la catégorie ne peut pas dépasser 200 caractères");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Le type de catégorie doit être INCOME ou EXPENSE");

        RuleFor(x => x.Icon)
            .MaximumLength(50)
            .WithMessage("L'icône ne peut pas dépasser 50 caractères")
            .When(x => x.Icon != null);
    }
}
