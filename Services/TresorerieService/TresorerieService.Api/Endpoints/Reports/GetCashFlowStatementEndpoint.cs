using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.Reports.Queries.GetCashFlowStatement;

namespace TresorerieService.Api.Endpoints.Reports;

/// <summary>
/// Endpoint pour recuperer l'etat des flux de tresorerie (cash flow statement)
/// US-035: Etat des flux de tresorerie
/// </summary>
public class GetCashFlowStatementEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/reports/cash-flow-statement", async (
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] bool comparePrevious = false,
            ISender sender = null!,
            CancellationToken cancellationToken = default) =>
        {
            // Validation des headers obligatoires
            if (string.IsNullOrEmpty(applicationId))
            {
                return Results.BadRequest(new { error = "L'en-tete X-Application-Id est obligatoire" });
            }

            if (string.IsNullOrEmpty(boutiqueId))
            {
                return Results.BadRequest(new { error = "L'en-tete X-Boutique-Id est obligatoire" });
            }

            // Validation des parametres de periode obligatoires
            if (!startDate.HasValue || !endDate.HasValue)
            {
                return Results.BadRequest(new { error = "startDate et endDate sont obligatoires" });
            }

            // Validation de coherence des dates
            if (startDate.Value > endDate.Value)
            {
                return Results.BadRequest(new { error = "startDate doit etre anterieure ou egale a endDate" });
            }

            var query = new GetCashFlowStatementQuery(
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                StartDate: startDate.Value,
                EndDate: endDate.Value,
                ComparePrevious: comparePrevious
            );

            var result = await sender.Send(query, cancellationToken);

            var response = ResponseFactory.Success(result, "Etat des flux de tresorerie recupere avec succes");

            return Results.Ok(response);
        })
        .WithName("GetCashFlowStatement")
        .WithTags("Reports")
        .Produces<BaseResponse<GetCashFlowStatementResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithSummary("Consulter l'etat des flux de tresorerie")
        .WithDescription("""
            Recupere un etat detaille des flux de tresorerie pour une periode donnee.

            L'etat inclut:
            - Total des entrees (INCOME): somme de tous les flux de type INCOME approuves
            - Total des sorties (EXPENSE): somme de tous les flux de type EXPENSE approuves
            - Solde net: difference entre les entrees et les sorties
            - Nombre de transactions par type
            - Repartition par categorie avec montant, pourcentage et nombre de transactions
            - Comparaison avec la periode precedente (optionnelle): variations en pourcentage

            Notes:
            - Seuls les flux avec le statut APPROVED sont comptabilises
            - Les transferts (TRANSFER) ne sont pas inclus dans les totaux
            - La periode precedente a la meme duree que la periode courante
            """)
        .RequireAuthorization();
    }
}
