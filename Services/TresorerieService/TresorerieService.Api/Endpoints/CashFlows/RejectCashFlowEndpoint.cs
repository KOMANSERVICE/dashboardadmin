using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.CashFlows.Commands.RejectCashFlow;
using TresorerieService.Application.Features.CashFlows.DTOs;

namespace TresorerieService.Api.Endpoints.CashFlows;

public record RejectCashFlowRequest(string RejectionReason);

public record RejectCashFlowResponse(CashFlowDTO CashFlow);

public class RejectCashFlowEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/cash-flows/{id:guid}/reject", async (
            [FromRoute] Guid id,
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
            [FromBody] RejectCashFlowRequest request,
            ISender sender,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrEmpty(applicationId))
            {
                return Results.BadRequest(new { error = "L'en-tete X-Application-Id est obligatoire" });
            }

            if (string.IsNullOrEmpty(boutiqueId))
            {
                return Results.BadRequest(new { error = "L'en-tete X-Boutique-Id est obligatoire" });
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

            var command = new RejectCashFlowCommand(
                CashFlowId: id,
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                RejectionReason: request.RejectionReason,
                RejectedBy: userId,
                UserRole: userRole
            );

            var result = await sender.Send(command, cancellationToken);

            var response = new RejectCashFlowResponse(result.CashFlow);
            var baseResponse = ResponseFactory.Success(response, "Flux rejete avec succes", StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("RejectCashFlow")
        .WithTags("CashFlows")
        .Produces<BaseResponse<RejectCashFlowResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Rejeter un flux de tresorerie soumis")
        .WithDescription("Rejette un flux de tresorerie en attente (PENDING). " +
            "Le statut passe de PENDING a REJECTED. " +
            "Un motif de rejet (rejectionReason) doit etre fourni (max 500 caracteres). " +
            "Le compte n'est PAS impacte. " +
            "Le budget n'est PAS impacte. " +
            "L'employe peut voir le motif de rejet. " +
            "Seul un manager ou admin peut rejeter un flux.")
        .RequireAuthorization();
    }
}
