using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.Categories.Queries.GetCategories;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Api.Endpoints.Categories;

public class GetCategoriesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/categories", async (
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromQuery] CategoryType? type = null,
            [FromQuery] bool includeInactive = false,
            ISender sender = null!,
            CancellationToken cancellationToken = default) =>
        {
            if (string.IsNullOrEmpty(applicationId))
            {
                return Results.BadRequest(new { error = "L'en-tete X-Application-Id est obligatoire" });
            }

            var query = new GetCategoriesQuery(
                ApplicationId: applicationId,
                Type: type,
                IncludeInactive: includeInactive
            );

            var result = await sender.Send(query, cancellationToken);

            var response = ResponseFactory.Success(result, "Liste des categories recuperee avec succes");

            return Results.Ok(response);
        })
        .WithName("GetCategories")
        .WithTags("Categories")
        .Produces<BaseResponse<GetCategoriesResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithSummary("Lister les categories de flux de tresorerie")
        .WithDescription("Recupere la liste des categories avec filtres optionnels par type (INCOME/EXPENSE) et statut actif/inactif. Les categories sont triees par nom.")
        .RequireAuthorization();
    }
}
