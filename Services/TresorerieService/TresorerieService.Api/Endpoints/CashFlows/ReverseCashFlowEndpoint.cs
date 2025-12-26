using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.CashFlows.Commands.ReverseCashFlow;

namespace TresorerieService.Api.Endpoints.CashFlows;

public record ReverseCashFlowRequest(
    string Reason,
    string? SourceType = null,
    Guid? SourceId = null
);

public class ReverseCashFlowEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/cash-flows/{id:guid}/reverse", async (
            [FromRoute] Guid id,
            [FromBody] ReverseCashFlowRequest request,
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
            ISender sender,
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

            var command = new ReverseCashFlowCommand(
                CashFlowId: id,
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                Reason: request.Reason,
                SourceType: request.SourceType,
                SourceId: request.SourceId
            );

            var result = await sender.Send(command, cancellationToken);

            var baseResponse = ResponseFactory.Success(result, "Contre-passation creee avec succes", StatusCodes.Status201Created);

            return Results.Created($"/api/cash-flows/{result.ReversalCashFlowId}", baseResponse);
        })
        .WithName("ReverseCashFlow")
        .WithTags("CashFlows")
        .Produces<BaseResponse<ReverseCashFlowResult>>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Creer une contre-passation pour un CashFlow")
        .WithDescription("Cree une contre-passation pour annuler un CashFlow approuve. " +
            "Le CashFlow original n'est jamais supprime, mais marque comme contre-passe (IsReversed = true). " +
            "Un nouveau CashFlow est cree avec le type inverse (INCOME devient EXPENSE et vice versa). " +
            "Le nouveau CashFlow reference le CashFlow original (OriginalCashFlowId) et est marque comme contre-passation (IsReversal = true). " +
            "Le solde du compte est automatiquement mis a jour pour annuler l'effet du flux original. " +
            "Si le flux original etait une depense liee a un budget, le SpentAmount du budget est egalement mis a jour.")
        .RequireAuthorization();
    }
}
