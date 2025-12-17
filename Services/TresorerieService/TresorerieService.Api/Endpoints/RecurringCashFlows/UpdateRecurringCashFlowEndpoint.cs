using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.RecurringCashFlows.Commands.UpdateRecurringCashFlow;
using TresorerieService.Application.Features.RecurringCashFlows.DTOs;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Api.Endpoints.RecurringCashFlows;

public record UpdateRecurringCashFlowRequest(
    CashFlowType? Type,
    string? CategoryId,
    string? Label,
    string? Description,
    decimal? Amount,
    Guid? AccountId,
    string? PaymentMethod,
    string? ThirdPartyName,
    RecurringFrequency? Frequency,
    int? Interval,
    int? DayOfMonth,
    int? DayOfWeek,
    DateTime? StartDate,
    DateTime? EndDate,
    bool? AutoValidate,
    bool? IsActive
);

public record UpdateRecurringCashFlowResponse(
    RecurringCashFlowDTO RecurringCashFlow
);

public class UpdateRecurringCashFlowEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/recurring-cash-flows/{id:guid}", async (
            [FromRoute] Guid id,
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
            [FromBody] UpdateRecurringCashFlowRequest request,
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

            var dto = new UpdateRecurringCashFlowDto(
                Type: request.Type,
                CategoryId: request.CategoryId,
                Label: request.Label,
                Description: request.Description,
                Amount: request.Amount,
                AccountId: request.AccountId,
                PaymentMethod: request.PaymentMethod,
                ThirdPartyName: request.ThirdPartyName,
                Frequency: request.Frequency,
                Interval: request.Interval,
                DayOfMonth: request.DayOfMonth,
                DayOfWeek: request.DayOfWeek,
                StartDate: request.StartDate,
                EndDate: request.EndDate,
                AutoValidate: request.AutoValidate,
                IsActive: request.IsActive
            );

            var command = new UpdateRecurringCashFlowCommand(
                Id: id,
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                UserId: userId,
                Data: dto
            );

            var result = await sender.Send(command, cancellationToken);

            var response = new UpdateRecurringCashFlowResponse(result.RecurringCashFlow);
            var baseResponse = ResponseFactory.Success(response, "Flux recurrent modifie avec succes", StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("UpdateRecurringCashFlow")
        .WithTags("RecurringCashFlows")
        .Produces<BaseResponse<UpdateRecurringCashFlowResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Modifier un flux de tresorerie recurrent")
        .WithDescription("Modifie un flux de tresorerie recurrent existant. " +
            "Champs NON modifiables: id, createdBy, createdAt. " +
            "Le type doit etre INCOME ou EXPENSE (pas TRANSFER). " +
            "Si la frequence est modifiee (frequency, interval, dayOfMonth, dayOfWeek), nextOccurrence est automatiquement recalculee. " +
            "Les flux deja generes ne sont pas affectes par cette modification.")
        .RequireAuthorization();
    }
}
