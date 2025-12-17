using FluentValidation;

namespace TresorerieService.Application.Features.Reports.Queries.GetCashFlowForecast;

/// <summary>
/// Validateur pour la query GetCashFlowForecast
/// US-036: Previsions de tresorerie
/// </summary>
public class GetCashFlowForecastValidator : AbstractValidator<GetCashFlowForecastQuery>
{
    public GetCashFlowForecastValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty()
            .WithMessage("ApplicationId est obligatoire");

        RuleFor(x => x.BoutiqueId)
            .NotEmpty()
            .WithMessage("BoutiqueId est obligatoire");

        RuleFor(x => x.Days)
            .InclusiveBetween(1, 90)
            .WithMessage("days doit etre entre 1 et 90");
    }
}
