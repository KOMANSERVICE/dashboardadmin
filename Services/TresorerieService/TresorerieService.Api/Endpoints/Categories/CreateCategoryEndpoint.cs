using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.Categories.Commands.CreateCategory;
using TresorerieService.Application.Features.Categories.DTOs;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Api.Endpoints.Categories;

public record CreateCategoryRequest(
    string Name,
    CategoryType Type,
    string? Icon
);

public record CreateCategoryResponse(CategoryDTO Category);

public class CreateCategoryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/categories", async (
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
            [FromBody] CreateCategoryRequest request,
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

            var command = new CreateCategoryCommand(
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                Name: request.Name,
                Type: request.Type,
                Icon: request.Icon
            );

            var result = await sender.Send(command, cancellationToken);

            var response = new CreateCategoryResponse(result.Category);
            var baseResponse = ResponseFactory.Success(response, "Catégorie créée avec succès", StatusCodes.Status201Created);

            return Results.Created($"/api/categories/{result.Category.Id}", baseResponse);
        })
        .WithName("CreateCategory")
        .WithTags("Categories")
        .Produces<BaseResponse<CreateCategoryResponse>>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithSummary("Créer une catégorie de flux de trésorerie")
        .WithDescription("Créer une nouvelle catégorie de type INCOME (revenu) ou EXPENSE (dépense) pour classifier les mouvements de trésorerie")
        .RequireAuthorization();
    }
}
