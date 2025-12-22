using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.Budgets.Queries.GetBudgetById;

namespace TresorerieService.Api.Endpoints.Budgets;

public class GetBudgetByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/budgets/{id:guid}", async (
            [FromRoute] Guid id,
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
            ISender sender,
            CancellationToken cancellationToken = default) =>
        {
            if (string.IsNullOrEmpty(applicationId))
            {
                return Results.BadRequest(new { error = "L'en-tete X-Application-Id est obligatoire" });
            }

            if (string.IsNullOrEmpty(boutiqueId))
            {
                return Results.BadRequest(new { error = "L'en-tete X-Boutique-Id est obligatoire" });
            }

            var query = new GetBudgetByIdQuery(
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                BudgetId: id
            );

            var result = await sender.Send(query, cancellationToken);

            var response = ResponseFactory.Success(result, "Detail du budget recupere avec succes");

            return Results.Ok(response);
        })
        .WithName("GetBudgetById")
        .WithTags("Budgets")
        .Produces<BaseResponse<GetBudgetByIdResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Consulter le detail d'un budget")
        .WithDescription("Recupere le detail complet d'un budget par son ID. " +
            "Inclut: allocatedAmount, spentAmount, remainingAmount, % utilise. " +
            "Inclut la liste des depenses (CashFlows EXPENSE) liees au budget. " +
            "Inclut la repartition par categorie. " +
            "Inclut l'evolution dans le temps (mensuelle). " +
            "Indique si le budget est depasse (isExceeded).")
        .RequireAuthorization();
    }
}
