using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.RecurringCashFlows.Queries.GetRecurringCashFlows;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Api.Endpoints.RecurringCashFlows;

public class GetRecurringCashFlowsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/recurring-cash-flows", async (
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
            [FromQuery] bool? isActive = true,
            [FromQuery] CashFlowType? type = null,
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

            var query = new GetRecurringCashFlowsQuery(
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                IsActive: isActive,
                Type: type
            );

            var result = await sender.Send(query, cancellationToken);

            var response = ResponseFactory.Success(result, "Liste des flux recurrents recuperee avec succes");

            return Results.Ok(response);
        })
        .WithName("GetRecurringCashFlows")
        .WithTags("RecurringCashFlows")
        .Produces<BaseResponse<GetRecurringCashFlowsResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithSummary("Lister les flux de tresorerie recurrents")
        .WithDescription("Recupere la liste des flux de tresorerie recurrents avec filtres. " +
            "Par defaut, seuls les flux actifs sont retournes (isActive=true). " +
            "Filtres disponibles: isActive (true/false), type (INCOME/EXPENSE). " +
            "La reponse inclut le montant total mensuel estime (estimatedMonthlyTotal) " +
            "calcule a partir de la frequence et de l'intervalle de chaque flux actif. " +
            "Formules: DAILY=amount*30/interval, WEEKLY=amount*4.33/interval, " +
            "MONTHLY=amount/interval, QUARTERLY=amount/3/interval, YEARLY=amount/12/interval.")
        .RequireAuthorization();
    }
}
