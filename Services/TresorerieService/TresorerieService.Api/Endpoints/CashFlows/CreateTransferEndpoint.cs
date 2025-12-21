using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.CashFlows.Commands.CreateTransfer;

namespace TresorerieService.Api.Endpoints.CashFlows;

public record CreateTransferRequest(
    Guid AccountId,
    Guid DestinationAccountId,
    decimal Amount,
    DateTime Date,
    string Label,
    string? Description
);

public record CreateTransferResponse(
    TransferDto Transfer
);

public class CreateTransferEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/cash-flows/transfer", async (
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
            [FromBody] CreateTransferRequest request,
            ISender sender,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
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

            var command = new CreateTransferCommand(
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                AccountId: request.AccountId,
                DestinationAccountId: request.DestinationAccountId,
                Amount: request.Amount,
                Date: request.Date,
                Label: request.Label,
                Description: request.Description,
                CreatedBy: userId
            );

            var result = await sender.Send(command, cancellationToken);

            var response = new CreateTransferResponse(result.Transfer);
            var baseResponse = ResponseFactory.Success(response, "Transfert cree avec succes", StatusCodes.Status201Created);

            return Results.Created($"/api/cash-flows/{result.Transfer.Id}", baseResponse);
        })
        .WithName("CreateTransfer")
        .WithTags("CashFlows")
        .Produces<BaseResponse<CreateTransferResponse>>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Creer un transfert entre deux comptes")
        .WithDescription("Cree un transfert de tresorerie entre un compte source et un compte destination. " +
            "Le transfert est automatiquement approuve (statut APPROVED). " +
            "Le compte source est debite et le compte destination est credite du montant specifie. " +
            "Un seul CashFlow est cree avec les deux comptes references.")
        .RequireAuthorization();
    }
}
