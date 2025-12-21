using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.CashFlows.Queries.GetCashFlowDetail;

namespace TresorerieService.Api.Endpoints.CashFlows;

public class GetCashFlowDetailEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/cash-flows/{id}", async (
            [FromRoute] Guid id,
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
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

            // Extraire le userId du token JWT
            var userId = httpContext.User.FindFirst("sub")?.Value
                         ?? httpContext.User.FindFirst("userId")?.Value
                         ?? httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                         ?? "unknown";

            // Extraire le role de l'utilisateur du token JWT
            var userRole = httpContext.User.FindFirst("role")?.Value
                           ?? httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                           ?? string.Empty;

            // Determiner si l'utilisateur est manager ou admin
            var isManager = userRole.Equals("manager", StringComparison.OrdinalIgnoreCase)
                            || userRole.Equals("admin", StringComparison.OrdinalIgnoreCase);

            var query = new GetCashFlowDetailQuery(
                CashFlowId: id,
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                UserId: userId,
                IsManager: isManager
            );

            var result = await sender.Send(query, cancellationToken);

            var response = ResponseFactory.Success(result, "Detail du flux de tresorerie recupere avec succes");

            return Results.Ok(response);
        })
        .WithName("GetCashFlowDetail")
        .WithTags("CashFlows")
        .Produces<BaseResponse<GetCashFlowDetailResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Consulter le detail d'un flux")
        .WithDescription("Recupere le detail complet d'un flux de tresorerie. " +
            "Inclut: tous les champs du flux, nom du compte source, nom du compte destination (si TRANSFER), " +
            "nom de la categorie, informations du createur, soumetteur et validateur/rejeteur. " +
            "Un employe ne peut voir que ses propres flux (CreatedBy = userId). " +
            "Un manager ou admin peut voir tous les flux de la boutique.")
        .RequireAuthorization();
    }
}
