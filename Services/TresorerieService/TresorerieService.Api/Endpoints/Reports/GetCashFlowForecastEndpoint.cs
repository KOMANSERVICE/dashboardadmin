using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.Reports.DTOs;
using TresorerieService.Application.Features.Reports.Queries.GetCashFlowForecast;

namespace TresorerieService.Api.Endpoints.Reports;

/// <summary>
/// Endpoint pour recuperer les previsions de tresorerie
/// US-036: Previsions de tresorerie
/// </summary>
public class GetCashFlowForecastEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/reports/cash-flow-forecast", async (
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
            [FromQuery] int days = 30,
            [FromQuery] bool includePending = true,
            ISender sender = null!,
            CancellationToken cancellationToken = default) =>
        {
            // Validation des headers obligatoires
            if (string.IsNullOrEmpty(applicationId))
            {
                return Results.BadRequest(new { error = "X-Application-Id est obligatoire" });
            }

            if (string.IsNullOrEmpty(boutiqueId))
            {
                return Results.BadRequest(new { error = "X-Boutique-Id est obligatoire" });
            }

            // Validation du parametre days
            if (days < 1 || days > 90)
            {
                return Results.BadRequest(new { error = "days doit etre entre 1 et 90" });
            }

            var query = new GetCashFlowForecastQuery(
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                Days: days,
                IncludePending: includePending
            );

            var result = await sender.Send(query, cancellationToken);

            var response = ResponseFactory.Success(result, "Previsions de tresorerie calculees avec succes");

            return Results.Ok(response);
        })
        .WithName("GetCashFlowForecast")
        .WithTags("Reports")
        .Produces<BaseResponse<CashFlowForecastDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Consulter les previsions de tresorerie")
        .WithDescription("""
            Recupere les previsions de tresorerie pour les N prochains jours.

            L'etat inclut:
            - Solde actuel et solde previsionnel en fin de periode
            - Previsions journalieres avec soldes d'ouverture et de fermeture
            - Flux recurrents pris en compte selon leur frequence (DAILY, WEEKLY, MONTHLY, etc.)
            - Flux en attente (PENDING) inclus par defaut (optionnel)
            - Avertissement si risque de tresorerie negative
            - Dates critiques ou la tresorerie sera negative ou sous le seuil d'alerte
            - Resume des totaux previsionnels

            Parametres:
            - days: Nombre de jours de prevision (1-90, defaut: 30)
            - includePending: Inclure les flux PENDING dans les previsions (defaut: true)
            """)
        .RequireAuthorization();
    }
}
