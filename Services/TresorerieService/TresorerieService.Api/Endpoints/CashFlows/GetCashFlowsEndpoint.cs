using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.CashFlows.Queries.GetCashFlows;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Api.Endpoints.CashFlows;

public class GetCashFlowsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/cash-flows", async (
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
            [FromQuery] CashFlowType? type,
            [FromQuery] CashFlowStatus? status,
            [FromQuery] Guid? accountId,
            [FromQuery] Guid? categoryId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string sortBy = "date",
            [FromQuery] string sortOrder = "desc",
            ISender sender = null!,
            HttpContext httpContext = null!,
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

            // Extraire le role de l'utilisateur du token JWT
            var userRole = httpContext.User.FindFirst("role")?.Value
                           ?? httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                           ?? string.Empty;

            // Determiner si l'utilisateur est manager ou admin
            var isManager = userRole.Equals("manager", StringComparison.OrdinalIgnoreCase)
                            || userRole.Equals("admin", StringComparison.OrdinalIgnoreCase);

            var query = new GetCashFlowsQuery(
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                IsManager: isManager,
                Type: type,
                Status: status,
                AccountId: accountId,
                CategoryId: categoryId,
                StartDate: startDate,
                EndDate: endDate,
                Search: search,
                Page: page,
                PageSize: pageSize,
                SortBy: sortBy,
                SortOrder: sortOrder
            );

            var result = await sender.Send(query, cancellationToken);

            var response = ResponseFactory.Success(result, "Liste des flux de tresorerie recuperee avec succes");

            return Results.Ok(response);
        })
        .WithName("GetCashFlows")
        .WithTags("CashFlows")
        .Produces<BaseResponse<GetCashFlowsResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithSummary("Lister les flux de tresorerie")
        .WithDescription("Recupere la liste des flux de tresorerie avec filtres, recherche et pagination. " +
            "Un employe voit uniquement ses propres flux (CreatedBy = userId). " +
            "Un manager ou admin voit tous les flux de la boutique. " +
            "Filtres disponibles: type (INCOME/EXPENSE/TRANSFER), status (DRAFT/PENDING/APPROVED/REJECTED/CANCELLED), " +
            "accountId, categoryId, startDate, endDate. " +
            "Recherche: sur label et reference. " +
            "Tri: date (defaut), amount, createdAt. " +
            "Pagination: 20 par page par defaut (max 100).")
        .RequireAuthorization();
    }
}
