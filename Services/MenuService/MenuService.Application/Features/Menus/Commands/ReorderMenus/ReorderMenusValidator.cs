namespace MenuService.Application.Features.Menus.Commands.ReorderMenus;

public class ReorderMenusValidator : AbstractValidator<ReorderMenusCommand>
{
    public ReorderMenusValidator()
    {
        RuleFor(x => x.AppAdminReference)
            .NotEmpty()
            .WithMessage("AppAdminReference is required.");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Items list cannot be empty.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Reference)
                .NotEmpty()
                .WithMessage("Menu reference is required.");

            item.RuleFor(i => i.SortOrder)
                .GreaterThanOrEqualTo(0)
                .WithMessage("SortOrder must be greater than or equal to 0.");
        });
    }
}
