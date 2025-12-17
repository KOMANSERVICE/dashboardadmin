using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.RecurringCashFlows.Commands.ToggleRecurringCashFlow;

namespace TresorerieService.Api.Endpoints.RecurringCashFlows;

public record ToggleRecurringCashFlowResponse(
    Guid Id,
    bool IsActive
);

public class ToggleRecurringCashFlowEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/recurring-cash-flows/{id:guid}/toggle", async (
            [FromRoute] Guid id,
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
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

            var command = new ToggleRecurringCashFlowCommand(
                Id: id,
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                UserId: userId
            );

            var result = await sender.Send(command, cancellationToken);

            var response = new ToggleRecurringCashFlowResponse(
                Id: result.Id,
                IsActive: result.IsActive
            );

            var message = result.IsActive
                ? "Flux recurrent reactive avec succes"
                : "Flux recurrent mis en pause avec succes";

            var baseResponse = ResponseFactory.Success(response, message, StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("ToggleRecurringCashFlow")
        .WithTags("RecurringCashFlows")
        .Produces<BaseResponse<ToggleRecurringCashFlowResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Activer/Desactiver un flux de tresorerie recurrent")
        .WithDescription("Bascule l'etat IsActive d'un flux de tresorerie recurrent. " +
            "Si le flux est actif (IsActive=true), il passe en pause (IsActive=false). " +
            "Si le flux est en pause (IsActive=false), il est reactive (IsActive=true). " +
            "Un flux en pause ne generera plus de nouveaux flux de tresorerie.")
        .RequireAuthorization();
    }
}
