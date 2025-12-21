using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.CashFlows.Commands.ReconcileCashFlows;

namespace TresorerieService.Api.Endpoints.CashFlows;

public record ReconcileCashFlowsRequest(
    IReadOnlyList<Guid> CashFlowIds,
    string? BankStatementReference
);

public record ReconcileCashFlowsResponse(
    int ReconciledCount,
    IReadOnlyList<ReconciledCashFlowDto> ReconciledCashFlows
);

public class ReconcileCashFlowsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/cash-flows/reconcile", async (
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
            [FromBody] ReconcileCashFlowsRequest request,
            ISender sender,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrEmpty(applicationId))
            {
                return Results.BadRequest(new { error = "X-Application-Id est obligatoire" });
            }

            if (string.IsNullOrEmpty(boutiqueId))
            {
                return Results.BadRequest(new { error = "X-Boutique-Id est obligatoire" });
            }

            if (request.CashFlowIds == null || request.CashFlowIds.Count == 0)
            {
                return Results.BadRequest(new { error = "Au moins un flux est requis" });
            }

            // Extraire le userId du token JWT
            var userId = httpContext.User.FindFirst("sub")?.Value
                         ?? httpContext.User.FindFirst("userId")?.Value
                         ?? httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                         ?? "unknown";

            // Extraire le role de l'utilisateur du token JWT
            var userRole = httpContext.User.FindFirst("role")?.Value
                           ?? httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                           ?? string.Empty;

            if (string.IsNullOrEmpty(userRole))
            {
                return Results.Forbid();
            }

            var command = new ReconcileCashFlowsCommand(
                CashFlowIds: request.CashFlowIds,
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                ReconciledBy: userId,
                UserRole: userRole,
                BankStatementReference: request.BankStatementReference
            );

            var result = await sender.Send(command, cancellationToken);

            var response = new ReconcileCashFlowsResponse(
                ReconciledCount: result.ReconciledCount,
                ReconciledCashFlows: result.ReconciledCashFlows
            );
            var baseResponse = ResponseFactory.Success(response, $"{result.ReconciledCount} flux reconcilies avec succes", StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("ReconcileCashFlows")
        .WithTags("CashFlows")
        .Produces<BaseResponse<ReconcileCashFlowsResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Reconcilier plusieurs flux de tresorerie en masse")
        .WithDescription("Reconcilie plusieurs flux de tresorerie approuves (APPROVED) en une seule operation. " +
            "Tous les flux doivent etre approuves et non encore reconcilies. " +
            "Une reference du releve bancaire optionnelle peut etre appliquee a tous les flux. " +
            "Seul un manager ou admin peut reconcilier des flux. " +
            "Retourne le nombre de flux reconcilies et la liste des flux traites.")
        .RequireAuthorization();
    }
}
