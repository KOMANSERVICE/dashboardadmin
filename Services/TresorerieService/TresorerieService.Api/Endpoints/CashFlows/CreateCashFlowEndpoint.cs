using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.CashFlows.Commands.CreateCashFlow;
using TresorerieService.Application.Features.CashFlows.DTOs;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Api.Endpoints.CashFlows;

public record CreateCashFlowRequest(
    CashFlowType Type,
    string CategoryId,
    string Label,
    string? Description,
    decimal Amount,
    Guid AccountId,
    string PaymentMethod,
    DateTime Date,
    string? CustomerName,
    string? SupplierName,
    string? AttachmentUrl
);

public record CreateCashFlowResponse(
    CashFlowDTO CashFlow,
    string? BudgetWarning
);

public class CreateCashFlowEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/cash-flows", async (
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
            [FromBody] CreateCashFlowRequest request,
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

            // Determiner le nom du tiers selon le type de flux
            var thirdPartyName = request.Type == CashFlowType.INCOME
                ? request.CustomerName
                : request.SupplierName;

            var command = new CreateCashFlowCommand(
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                Type: request.Type,
                CategoryId: request.CategoryId,
                Label: request.Label,
                Description: request.Description,
                Amount: request.Amount,
                AccountId: request.AccountId,
                PaymentMethod: request.PaymentMethod,
                Date: request.Date,
                ThirdPartyName: thirdPartyName,
                AttachmentUrl: request.AttachmentUrl,
                CreatedBy: userId
            );

            var result = await sender.Send(command, cancellationToken);

            var successMessage = request.Type == CashFlowType.INCOME
                ? "Revenu cree avec succes en brouillon"
                : "Depense creee avec succes en brouillon";

            var response = new CreateCashFlowResponse(result.CashFlow, result.BudgetWarning);
            var baseResponse = ResponseFactory.Success(response, successMessage, StatusCodes.Status201Created);

            return Results.Created($"/api/cash-flows/{result.CashFlow.Id}", baseResponse);
        })
        .WithName("CreateCashFlow")
        .WithTags("CashFlows")
        .Produces<BaseResponse<CreateCashFlowResponse>>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Creer un flux de tresorerie (revenu ou depense)")
        .WithDescription("Cree un nouveau flux de tresorerie en mode brouillon (DRAFT). " +
            "Pour un revenu (INCOME): utiliser customerName et une categorie de type INCOME. " +
            "Pour une depense (EXPENSE): utiliser supplierName et une categorie de type EXPENSE. " +
            "Le statut est automatiquement defini sur DRAFT. Aucun budget n'est applicable pour les revenus (budgetId = null).")
        .RequireAuthorization();
    }
}
