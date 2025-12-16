using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.CashFlows.Commands.CreateCashFlowFromPurchase;
using TresorerieService.Application.Features.CashFlows.DTOs;

namespace TresorerieService.Api.Endpoints.CashFlows;

/// <summary>
/// Request DTO pour creer un flux de tresorerie depuis un achat.
/// </summary>
public record CreateCashFlowFromPurchaseRequest(
    Guid PurchaseId,
    string PurchaseReference,
    decimal Amount,
    Guid AccountId,
    string PaymentMethod,
    DateTime PurchaseDate,
    string? SupplierName,
    string? SupplierId,
    string CategoryId
);

/// <summary>
/// Response DTO pour la creation d'un flux de tresorerie depuis un achat.
/// </summary>
public record CreateCashFlowFromPurchaseResponse(
    CashFlowDTO CashFlow,
    decimal NewAccountBalance
);

/// <summary>
/// Endpoint Carter pour creer automatiquement un flux de tresorerie
/// quand un achat est valide (US-020).
/// Route: POST /api/cash-flows/from-purchase
/// </summary>
public class CreateCashFlowFromPurchaseEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/cash-flows/from-purchase", async (
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
            [FromBody] CreateCashFlowFromPurchaseRequest request,
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

            var command = new CreateCashFlowFromPurchaseCommand(
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                PurchaseId: request.PurchaseId,
                PurchaseReference: request.PurchaseReference,
                Amount: request.Amount,
                AccountId: request.AccountId,
                PaymentMethod: request.PaymentMethod,
                PurchaseDate: request.PurchaseDate,
                SupplierName: request.SupplierName,
                SupplierId: request.SupplierId,
                CategoryId: request.CategoryId
            );

            var result = await sender.Send(command, cancellationToken);

            var response = new CreateCashFlowFromPurchaseResponse(result.CashFlow, result.NewAccountBalance);
            var baseResponse = ResponseFactory.Success(
                response,
                $"Flux de tresorerie cree automatiquement pour l'achat #{request.PurchaseReference}",
                StatusCodes.Status201Created);

            return Results.Created($"/api/cash-flows/{result.CashFlow.Id}", baseResponse);
        })
        .WithName("CreateCashFlowFromPurchase")
        .WithTags("CashFlows")
        .Produces<BaseResponse<CreateCashFlowFromPurchaseResponse>>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .WithSummary("Creer automatiquement un flux de tresorerie depuis un achat")
        .WithDescription(
            "Cree automatiquement un flux de tresorerie (CashFlow) quand un achat est valide. " +
            "Le flux est cree avec Type=EXPENSE et Status=APPROVED. " +
            "Le compte source est automatiquement debite du montant de l'achat. " +
            "Un doublon est detecte si un flux existe deja pour le meme achat (RelatedType=PURCHASE, RelatedId=PurchaseId).")
        .RequireAuthorization();
    }
}
