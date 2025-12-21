using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.CashFlows.Commands.ReconcileCashFlow;
using TresorerieService.Application.Features.CashFlows.DTOs;

namespace TresorerieService.Api.Endpoints.CashFlows;

public record ReconcileCashFlowRequest(
    string? BankStatementReference
);

public record ReconcileCashFlowResponse(
    CashFlowDetailDto CashFlow
);

public class ReconcileCashFlowEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/cash-flows/{id:guid}/reconcile", async (
            [FromRoute] Guid id,
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
            [FromBody] ReconcileCashFlowRequest? request,
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

            var command = new ReconcileCashFlowCommand(
                CashFlowId: id,
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                ReconciledBy: userId,
                UserRole: userRole,
                BankStatementReference: request?.BankStatementReference
            );

            var result = await sender.Send(command, cancellationToken);

            var response = new ReconcileCashFlowResponse(result.CashFlow);
            var baseResponse = ResponseFactory.Success(response, "Flux reconcilie avec succes", StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("ReconcileCashFlow")
        .WithTags("CashFlows")
        .Produces<BaseResponse<ReconcileCashFlowResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Marquer un flux de tresorerie comme reconcilie")
        .WithDescription("Marque un flux de tresorerie approuve (APPROVED) comme reconcilie avec le releve bancaire. " +
            "Le champ IsReconciled passe a true. " +
            "La date de reconciliation (reconciledAt) est enregistree automatiquement. " +
            "Une reference du releve bancaire peut etre saisie optionnellement. " +
            "Seul un manager ou admin peut reconcilier un flux.")
        .RequireAuthorization();
    }
}
