using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.CashFlows.Commands.UpdateCashFlow;
using TresorerieService.Application.Features.CashFlows.DTOs;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Api.Endpoints.CashFlows;

public record UpdateCashFlowRequest(
    string? CategoryId,
    string? Label,
    string? Description,
    decimal? Amount,
    decimal? TaxAmount,
    decimal? TaxRate,
    string? Currency,
    Guid? AccountId,
    Guid? DestinationAccountId,
    string? PaymentMethod,
    DateTime? Date,
    ThirdPartyType? ThirdPartyType,
    string? ThirdPartyName,
    string? ThirdPartyId,
    string? AttachmentUrl
);

public record UpdateCashFlowResponse(CashFlowDTO CashFlow);

public class UpdateCashFlowEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/cash-flows/{id:guid}", async (
            [FromRoute] Guid id,
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
            [FromBody] UpdateCashFlowRequest request,
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

            var dto = new UpdateCashFlowDto(
                CategoryId: request.CategoryId,
                Label: request.Label,
                Description: request.Description,
                Amount: request.Amount,
                TaxAmount: request.TaxAmount,
                TaxRate: request.TaxRate,
                Currency: request.Currency,
                AccountId: request.AccountId,
                DestinationAccountId: request.DestinationAccountId,
                PaymentMethod: request.PaymentMethod,
                Date: request.Date,
                ThirdPartyType: request.ThirdPartyType,
                ThirdPartyName: request.ThirdPartyName,
                ThirdPartyId: request.ThirdPartyId,
                AttachmentUrl: request.AttachmentUrl
            );

            var command = new UpdateCashFlowCommand(
                Id: id,
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                UserId: userId,
                Data: dto
            );

            var result = await sender.Send(command, cancellationToken);

            var response = new UpdateCashFlowResponse(result.CashFlow);
            var baseResponse = ResponseFactory.Success(response, "Flux modifie avec succes", StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("UpdateCashFlow")
        .WithTags("CashFlows")
        .Produces<BaseResponse<UpdateCashFlowResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Modifier un flux de tresorerie en brouillon")
        .WithDescription("Modifie un flux de tresorerie existant. " +
            "Seuls les flux avec le statut DRAFT peuvent etre modifies. " +
            "L'utilisateur ne peut modifier que ses propres flux. " +
            "Les champs non modifiables sont: id, type, createdBy, createdAt.")
        .RequireAuthorization();
    }
}
