using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.Accounts.Commands.UpdateAccount;
using TresorerieService.Application.Features.Accounts.DTOs;

namespace TresorerieService.Api.Endpoints.Accounts;

public record UpdateAccountRequest(
    string Name,
    decimal? AlertThreshold,
    decimal? OverdraftLimit,
    bool IsActive,
    decimal? InitialBalance = null
);

public record UpdateAccountResponse(Guid Id);

public class UpdateAccountEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/accounts/{accountId:guid}", async (
            Guid accountId,
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
            [FromBody] UpdateAccountRequest request,
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

            var updateDto = new UpdateAccountDTO(
                Name: request.Name,
                AlertThreshold: request.AlertThreshold,
                OverdraftLimit: request.OverdraftLimit,
                IsActive: request.IsActive,
                InitialBalance: request.InitialBalance
            );

            var command = new UpdateAccountCommand(
                AccountId: accountId,
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                Data: updateDto
            );

            var result = await sender.Send(command, cancellationToken);

            var response = new UpdateAccountResponse(result.Id);
            var baseResponse = ResponseFactory.Success(response, "Compte modifié avec succès", StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("UpdateAccount")
        .WithTags("Accounts")
        .Produces<BaseResponse<UpdateAccountResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Modifier un compte de trésorerie")
        .WithDescription("Modifier un compte existant. Le type ne peut pas être modifié. Le solde initial ne peut être modifié que s'il n'y a pas de flux enregistrés.")
        .RequireAuthorization();
    }
}
