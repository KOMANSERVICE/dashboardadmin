using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.CashFlows.Queries.GetUnreconciledCashFlows;

namespace TresorerieService.Api.Endpoints.CashFlows;

public class GetUnreconciledCashFlowsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/cash-flows/unreconciled", async (
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
            [FromQuery] Guid? accountId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            ISender sender = null!,
            CancellationToken cancellationToken = default) =>
        {
            if (string.IsNullOrEmpty(applicationId))
            {
                return Results.BadRequest(new { error = "X-Application-Id est obligatoire" });
            }

            if (string.IsNullOrEmpty(boutiqueId))
            {
                return Results.BadRequest(new { error = "X-Boutique-Id est obligatoire" });
            }

            var query = new GetUnreconciledCashFlowsQuery(
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                AccountId: accountId,
                StartDate: startDate,
                EndDate: endDate
            );

            var result = await sender.Send(query, cancellationToken);

            var response = ResponseFactory.Success(result, "Liste des flux non reconcilies recuperee avec succes");

            return Results.Ok(response);
        })
        .WithName("GetUnreconciledCashFlows")
        .WithTags("CashFlows")
        .Produces<BaseResponse<GetUnreconciledCashFlowsResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithSummary("Lister les flux non reconcilies")
        .WithDescription("Recupere la liste des flux de tresorerie non reconcilies (Status = APPROVED et IsReconciled = false). " +
            "Les flux sont tries par date (les plus anciens en premier). " +
            "Filtres optionnels: accountId, startDate, endDate. " +
            "Retourne le nombre de flux non reconcilies et le montant total non reconcilie.")
        .RequireAuthorization();
    }
}
