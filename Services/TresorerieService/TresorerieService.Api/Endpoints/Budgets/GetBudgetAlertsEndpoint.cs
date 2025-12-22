using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.Budgets.Queries.GetBudgetAlerts;

namespace TresorerieService.Api.Endpoints.Budgets;

public class GetBudgetAlertsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/budgets/alerts", async (
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
            ISender sender = null!,
            CancellationToken cancellationToken = default) =>
        {
            if (string.IsNullOrEmpty(applicationId))
            {
                return Results.BadRequest(new { error = "L'en-tete X-Application-Id est obligatoire" });
            }

            if (string.IsNullOrEmpty(boutiqueId))
            {
                return Results.BadRequest(new { error = "L'en-tete X-Boutique-Id est obligatoire" });
            }

            var query = new GetBudgetAlertsQuery(
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId
            );

            var result = await sender.Send(query, cancellationToken);

            var response = ResponseFactory.Success(result, "Alertes budgetaires recuperees avec succes");

            return Results.Ok(response);
        })
        .WithName("GetBudgetAlerts")
        .WithTags("Budgets")
        .Produces<BaseResponse<GetBudgetAlertsResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithSummary("Voir les alertes budgetaires")
        .WithDescription("Recupere la liste des alertes budgetaires de la boutique. " +
            "Affiche les budgets ayant atteint le seuil d'alerte (APPROACHING) ou depasses (EXCEEDED). " +
            "Les alertes sont triees par criticite: depasses en premier, puis par taux de consommation decroissant. " +
            "Inclut un resume avec le nombre total d'alertes, depasses et proches du seuil.")
        .RequireAuthorization();
    }
}
