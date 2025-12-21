using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.CashFlows.Commands.CreateCashFlowFromSale;
using TresorerieService.Application.Features.CashFlows.DTOs;

namespace TresorerieService.Api.Endpoints.CashFlows;

/// <summary>
/// Request DTO pour creer un flux de tresorerie depuis une vente.
/// </summary>
public record CreateCashFlowFromSaleRequest(
    Guid SaleId,
    string SaleReference,
    decimal Amount,
    Guid AccountId,
    string PaymentMethod,
    DateTime SaleDate,
    string? CustomerName,
    string? CustomerId,
    string CategoryId
);

/// <summary>
/// Response DTO pour la creation d'un flux de tresorerie depuis une vente.
/// </summary>
public record CreateCashFlowFromSaleResponse(
    CashFlowDTO CashFlow,
    decimal NewAccountBalance
);

/// <summary>
/// Endpoint Carter pour creer automatiquement un flux de tresorerie
/// quand une vente est validee (US-019).
/// Route: POST /api/cash-flows/from-sale
/// </summary>
public class CreateCashFlowFromSaleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/cash-flows/from-sale", async (
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
            [FromBody] CreateCashFlowFromSaleRequest request,
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

            var command = new CreateCashFlowFromSaleCommand(
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                SaleId: request.SaleId,
                SaleReference: request.SaleReference,
                Amount: request.Amount,
                AccountId: request.AccountId,
                PaymentMethod: request.PaymentMethod,
                SaleDate: request.SaleDate,
                CustomerName: request.CustomerName,
                CustomerId: request.CustomerId,
                CategoryId: request.CategoryId
            );

            var result = await sender.Send(command, cancellationToken);

            var response = new CreateCashFlowFromSaleResponse(result.CashFlow, result.NewAccountBalance);
            var baseResponse = ResponseFactory.Success(
                response,
                $"Flux de tresorerie cree automatiquement pour la vente #{request.SaleReference}",
                StatusCodes.Status201Created);

            return Results.Created($"/api/cash-flows/{result.CashFlow.Id}", baseResponse);
        })
        .WithName("CreateCashFlowFromSale")
        .WithTags("CashFlows")
        .Produces<BaseResponse<CreateCashFlowFromSaleResponse>>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .WithSummary("Creer automatiquement un flux de tresorerie depuis une vente")
        .WithDescription(
            "Cree automatiquement un flux de tresorerie (CashFlow) quand une vente est validee. " +
            "Le flux est cree avec Type=INCOME et Status=APPROVED. " +
            "Le compte de destination est automatiquement credite du montant de la vente. " +
            "Un doublon est detecte si un flux existe deja pour la meme vente (RelatedType=SALE, RelatedId=SaleId).")
        .RequireAuthorization();
    }
}
