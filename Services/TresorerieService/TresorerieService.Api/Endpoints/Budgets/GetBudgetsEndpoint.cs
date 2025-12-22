using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.Budgets.Queries.GetBudgets;

namespace TresorerieService.Api.Endpoints.Budgets;

public class GetBudgetsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/budgets", async (
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
            [FromQuery] bool? isActive = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            ISender sender = null!,
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

            var query = new GetBudgetsQuery(
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                IsActive: isActive,
                StartDate: startDate,
                EndDate: endDate
            );

            var result = await sender.Send(query, cancellationToken);

            var response = ResponseFactory.Success(result, "Liste des budgets recuperee avec succes");

            return Results.Ok(response);
        })
        .WithName("GetBudgets")
        .WithTags("Budgets")
        .Produces<BaseResponse<GetBudgetsResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithSummary("Lister les budgets")
        .WithDescription("Recupere la liste des budgets de la boutique. " +
            "Pour chaque budget: nom, periode, montant alloue, montant depense, % utilise. " +
            "Les budgets depasses (IsExceeded) et proches du seuil d'alerte (IsNearAlert) sont signales. " +
            "Filtres: isActive (true/false), startDate et endDate pour filtrer par periode.")
        .RequireAuthorization();
    }
}
