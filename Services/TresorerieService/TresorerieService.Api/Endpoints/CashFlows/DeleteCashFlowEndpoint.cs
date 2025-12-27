using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.CashFlows.Commands.DeleteCashFlow;

namespace TresorerieService.Api.Endpoints.CashFlows;

public class DeleteCashFlowEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/cash-flows/{id:guid}", async (
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

            var command = new DeleteCashFlowCommand(
                Id: id,
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId
            );

            await sender.Send(command, cancellationToken);

            return Results.NoContent();
        })
        .WithName("DeleteCashFlow")
        .WithTags("CashFlows")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Supprimer un flux de tresorerie en brouillon")
        .WithDescription("Supprime un flux de tresorerie existant. " +
            "Seuls les flux avec le statut DRAFT peuvent etre supprimes. " +
            "L'utilisateur ne peut supprimer que ses propres flux. " +
            "La suppression est definitive.")
        .RequireAuthorization();
    }
}
