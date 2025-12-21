using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.CashFlows.Commands.ApproveCashFlow;
using TresorerieService.Application.Features.CashFlows.DTOs;

namespace TresorerieService.Api.Endpoints.CashFlows;

public record ApproveCashFlowResponse(
    CashFlowDTO CashFlow,
    decimal NewAccountBalance
);

public class ApproveCashFlowEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/cash-flows/{id:guid}/approve", async (
            [FromRoute] Guid id,
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
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

            var command = new ApproveCashFlowCommand(
                CashFlowId: id,
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                ValidatedBy: userId,
                UserRole: userRole
            );

            var result = await sender.Send(command, cancellationToken);

            var response = new ApproveCashFlowResponse(result.CashFlow, result.NewAccountBalance);
            var baseResponse = ResponseFactory.Success(response, "Flux valide avec succes", StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("ApproveCashFlow")
        .WithTags("CashFlows")
        .Produces<BaseResponse<ApproveCashFlowResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Valider un flux de tresorerie soumis")
        .WithDescription("Valide un flux de tresorerie en attente (PENDING). " +
            "Le statut passe de PENDING a APPROVED. " +
            "La date de validation (validatedAt) et le validateur (validatedBy) sont enregistres. " +
            "Pour INCOME: Le compte est credite du montant. " +
            "Pour EXPENSE: Le compte est debite du montant. " +
            "Pour TRANSFER: Deja approuve automatiquement (ne passe pas par ce workflow). " +
            "Seul un manager ou admin peut valider un flux.")
        .RequireAuthorization();
    }
}
