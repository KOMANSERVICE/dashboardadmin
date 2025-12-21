using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.CashFlows.Commands.SubmitCashFlow;
using TresorerieService.Application.Features.CashFlows.DTOs;

namespace TresorerieService.Api.Endpoints.CashFlows;

public record SubmitCashFlowResponse(
    CashFlowDTO CashFlow,
    string? BudgetWarning
);

public class SubmitCashFlowEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/cash-flows/{id:guid}/submit", async (
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

            var command = new SubmitCashFlowCommand(
                CashFlowId: id,
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                SubmittedBy: userId
            );

            var result = await sender.Send(command, cancellationToken);

            var response = new SubmitCashFlowResponse(result.CashFlow, result.BudgetWarning);
            var baseResponse = ResponseFactory.Success(response, "Flux soumis pour validation avec succes", StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("SubmitCashFlow")
        .WithTags("CashFlows")
        .Produces<BaseResponse<SubmitCashFlowResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Soumettre un flux de tresorerie pour validation")
        .WithDescription("Soumet un flux de tresorerie en brouillon (DRAFT) pour validation par un manager. " +
            "Le statut passe de DRAFT a PENDING. " +
            "La date de soumission (submittedAt) est enregistree. " +
            "Une fois soumis, le flux ne peut plus etre modifie. " +
            "Si le type est EXPENSE et qu'un budget est defini, un avertissement est retourne si le budget sera depasse.")
        .RequireAuthorization();
    }
}
