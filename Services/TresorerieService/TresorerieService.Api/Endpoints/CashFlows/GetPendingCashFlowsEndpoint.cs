using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.CashFlows.Queries.GetPendingCashFlows;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Api.Endpoints.CashFlows;

public class GetPendingCashFlowsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/cash-flows/pending", async (
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
            [FromQuery] CashFlowType? type,
            [FromQuery] Guid? accountId,
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

            var query = new GetPendingCashFlowsQuery(
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                Type: type,
                AccountId: accountId
            );

            var result = await sender.Send(query, cancellationToken);

            var response = ResponseFactory.Success(result, "Liste des flux en attente de validation recuperee avec succes");

            return Results.Ok(response);
        })
        .WithName("GetPendingCashFlows")
        .WithTags("CashFlows")
        .Produces<BaseResponse<GetPendingCashFlowsResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithSummary("Lister les flux en attente de validation")
        .WithDescription("Recupere la liste des flux de tresorerie en attente de validation (Status = PENDING). " +
            "Les flux sont tries par date de soumission (les plus anciens en premier). " +
            "Affiche le nom de l'employe qui a soumis le flux. " +
            "Filtres optionnels: type (INCOME/EXPENSE/TRANSFER), accountId. " +
            "Un compteur indique le nombre total de flux en attente.")
        .RequireAuthorization();
    }
}
