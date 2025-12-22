using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.Budgets.Commands.UpdateBudget;
using TresorerieService.Application.Features.Budgets.DTOs;

namespace TresorerieService.Api.Endpoints.Budgets;

public record UpdateBudgetRequest(
    string? Name,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal? AllocatedAmount,
    List<Guid>? CategoryIds,
    int? AlertThreshold
);

public record UpdateBudgetResponse(
    BudgetDTO Budget,
    string? Warning
);

public class UpdateBudgetEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/budgets/{id:guid}", async (
            [FromRoute] Guid id,
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
            [FromBody] UpdateBudgetRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrEmpty(applicationId))
            {
                return Results.BadRequest(new { error = "L'en-tête X-Application-Id est obligatoire" });
            }

            if (string.IsNullOrEmpty(boutiqueId))
            {
                return Results.BadRequest(new { error = "L'en-tête X-Boutique-Id est obligatoire" });
            }

            var command = new UpdateBudgetCommand(
                Id: id,
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                Name: request.Name,
                StartDate: request.StartDate,
                EndDate: request.EndDate,
                AllocatedAmount: request.AllocatedAmount,
                CategoryIds: request.CategoryIds,
                AlertThreshold: request.AlertThreshold
            );

            var result = await sender.Send(command, cancellationToken);

            var response = new UpdateBudgetResponse(result.Budget, result.Warning);

            if (!string.IsNullOrEmpty(result.Warning))
            {
                var warningResponse = ResponseFactory.Success(response, result.Warning, StatusCodes.Status200OK);
                return Results.Ok(warningResponse);
            }

            var baseResponse = ResponseFactory.Success(response, "Budget modifié avec succès", StatusCodes.Status200OK);
            return Results.Ok(baseResponse);
        })
        .WithName("UpdateBudget")
        .WithTags("Budgets")
        .Produces<BaseResponse<UpdateBudgetResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Modifier un budget")
        .WithDescription("Modifier un budget existant. Champs modifiables: name, allocatedAmount, endDate, alertThreshold, categoryIds. La modification de startDate n'est pas autorisée si des dépenses existent. Si allocatedAmount est réduit sous spentAmount, un avertissement est retourné.")
        .RequireAuthorization();
    }
}
