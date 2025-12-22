using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.Budgets.Commands.CreateBudget;
using TresorerieService.Application.Features.Budgets.DTOs;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Api.Endpoints.Budgets;

public record CreateBudgetRequest(
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    decimal AllocatedAmount,
    List<Guid>? CategoryIds,
    int AlertThreshold,
    BudgetType Type
);

public record CreateBudgetResponse(BudgetDTO Budget);

public class CreateBudgetEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/budgets", async (
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
            [FromBody] CreateBudgetRequest request,
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

            var command = new CreateBudgetCommand(
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                Name: request.Name,
                StartDate: request.StartDate,
                EndDate: request.EndDate,
                AllocatedAmount: request.AllocatedAmount,
                CategoryIds: request.CategoryIds ?? new List<Guid>(),
                AlertThreshold: request.AlertThreshold,
                Type: request.Type
            );

            var result = await sender.Send(command, cancellationToken);

            var response = new CreateBudgetResponse(result.Budget);
            var baseResponse = ResponseFactory.Success(response, "Budget créé avec succès", StatusCodes.Status201Created);

            return Results.Created($"/api/budgets/{result.Budget.Id}", baseResponse);
        })
        .WithName("CreateBudget")
        .WithTags("Budgets")
        .Produces<BaseResponse<CreateBudgetResponse>>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithSummary("Créer un budget")
        .WithDescription("Créer un nouveau budget pour contrôler les dépenses sur une période. Types disponibles: GLOBAL, CATEGORY, PROJECT")
        .RequireAuthorization();
    }
}
