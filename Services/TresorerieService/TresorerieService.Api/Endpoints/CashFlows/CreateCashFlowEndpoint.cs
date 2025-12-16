using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.CashFlows.Commands.CreateCashFlow;
using TresorerieService.Application.Features.CashFlows.DTOs;

namespace TresorerieService.Api.Endpoints.CashFlows;

public record CreateCashFlowRequest(
    string CategoryId,
    string Label,
    string? Description,
    decimal Amount,
    Guid AccountId,
    string PaymentMethod,
    DateTime Date,
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

            var command = new CreateCashFlowCommand(
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                CategoryId: request.CategoryId,
                Label: request.Label,
                Description: request.Description,
                Amount: request.Amount,
                AccountId: request.AccountId,
                PaymentMethod: request.PaymentMethod,
                Date: request.Date,
                SupplierName: request.SupplierName,
                AttachmentUrl: request.AttachmentUrl,
                CreatedBy: userId
            );

            var result = await sender.Send(command, cancellationToken);

            var response = new CreateCashFlowResponse(result.CashFlow, result.BudgetWarning);
            var baseResponse = ResponseFactory.Success(response, "Depense creee avec succes en brouillon", StatusCodes.Status201Created);

            return Results.Created($"/api/cash-flows/{result.CashFlow.Id}", baseResponse);
        })
        .WithName("CreateCashFlow")
        .WithTags("CashFlows")
        .Produces<BaseResponse<CreateCashFlowResponse>>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Creer un flux de tresorerie (depense)")
        .WithDescription("Cree une nouvelle depense en mode brouillon (DRAFT). La depense doit etre soumise pour validation. Le type est automatiquement defini sur EXPENSE et le statut sur DRAFT.")
        .RequireAuthorization();
    }
}
