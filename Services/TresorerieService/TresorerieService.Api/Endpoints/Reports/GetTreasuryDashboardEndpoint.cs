using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.Reports.DTOs;
using TresorerieService.Application.Features.Reports.Queries.GetTreasuryDashboard;

namespace TresorerieService.Api.Endpoints.Reports;

/// <summary>
/// Endpoint pour recuperer le tableau de bord de tresorerie
/// </summary>
public class GetTreasuryDashboardEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/reports/treasury-dashboard", async (
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

            var query = new GetTreasuryDashboardQuery(
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId
            );

            var result = await sender.Send(query, cancellationToken);

            var response = ResponseFactory.Success(result, "Tableau de bord de tresorerie recupere avec succes");

            return Results.Ok(response);
        })
        .WithName("GetTreasuryDashboard")
        .WithTags("Reports")
        .Produces<BaseResponse<TreasuryDashboardDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithSummary("Consulter le tableau de bord de tresorerie")
        .WithDescription("""
            Recupere une vue d'ensemble de la tresorerie incluant:
            - Total de tresorerie disponible (tous comptes actifs)
            - Total par type de compte (CASH, BANK, MOBILE_MONEY)
            - Revenus et depenses du mois courant (statut APPROVED uniquement)
            - Solde net du mois (revenus - depenses)
            - Flux en attente de validation (statut PENDING)
            - Alertes (comptes dont le solde est inferieur au seuil configure)
            - Evolution du solde sur les 6 derniers mois
            """)
        .RequireAuthorization();
    }
}
