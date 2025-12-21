using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.RecurringCashFlows.Commands.CreateRecurringCashFlow;
using TresorerieService.Application.Features.RecurringCashFlows.DTOs;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Api.Endpoints.RecurringCashFlows;

public record CreateRecurringCashFlowRequest(
    CashFlowType Type,
    string CategoryId,
    string Label,
    string? Description,
    decimal Amount,
    Guid AccountId,
    string PaymentMethod,
    string? ThirdPartyName,
    RecurringFrequency Frequency,
    int Interval,
    int? DayOfMonth,
    int? DayOfWeek,
    DateTime StartDate,
    DateTime? EndDate,
    bool AutoValidate
);

public record CreateRecurringCashFlowResponse(
    RecurringCashFlowDTO RecurringCashFlow
);

public class CreateRecurringCashFlowEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/recurring-cash-flows", async (
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
            [FromBody] CreateRecurringCashFlowRequest request,
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

            var command = new CreateRecurringCashFlowCommand(
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
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
                CreatedBy: userId
            );

            var result = await sender.Send(command, cancellationToken);

            var response = new CreateRecurringCashFlowResponse(result.RecurringCashFlow);
            var baseResponse = ResponseFactory.Success(response, "Flux recurrent cree avec succes", StatusCodes.Status201Created);

            return Results.Created($"/api/recurring-cash-flows/{result.RecurringCashFlow.Id}", baseResponse);
        })
        .WithName("CreateRecurringCashFlow")
        .WithTags("RecurringCashFlows")
        .Produces<BaseResponse<CreateRecurringCashFlowResponse>>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Creer un flux de tresorerie recurrent")
        .WithDescription("Cree un nouveau flux de tresorerie recurrent pour automatiser les flux reguliers (loyer, salaires, abonnements). " +
            "Le type doit etre INCOME ou EXPENSE (pas TRANSFER). " +
            "Pour une frequence MONTHLY, le dayOfMonth (1-31) est obligatoire. " +
            "Pour une frequence WEEKLY, le dayOfWeek (1-7, 1=Lundi) est obligatoire. " +
            "La prochaine occurrence (nextOccurrence) est calculee automatiquement. " +
            "Si autoValidate est true, les flux generes seront automatiquement approuves.")
        .RequireAuthorization();
    }
}
