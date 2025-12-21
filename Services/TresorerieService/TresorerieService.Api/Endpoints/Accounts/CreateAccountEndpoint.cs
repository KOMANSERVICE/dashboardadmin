using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.Accounts.Commands.CreateAccount;
using TresorerieService.Application.Features.Accounts.DTOs;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Api.Endpoints.Accounts;

public record CreateAccountRequest(
    string Name,
    string? Description,
    AccountType Type,
    decimal InitialBalance,
    decimal? AlertThreshold,
    decimal? OverdraftLimit,
    string? AccountNumber,
    string? BankName,
    bool IsDefault
);

public record CreateAccountResponse(AccountDTO Account);

public class CreateAccountEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/accounts", async (
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
            [FromBody] CreateAccountRequest request,
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

            var command = new CreateAccountCommand(
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                Name: request.Name,
                Description: request.Description,
                Type: request.Type,
                InitialBalance: request.InitialBalance,
                AlertThreshold: request.AlertThreshold,
                OverdraftLimit: request.OverdraftLimit,
                AccountNumber: request.AccountNumber,
                BankName: request.BankName,
                IsDefault: request.IsDefault
            );

            var result = await sender.Send(command, cancellationToken);

            var response = new CreateAccountResponse(result.Account);
            var baseResponse = ResponseFactory.Success(response, "Compte créé avec succès", StatusCodes.Status201Created);

            return Results.Created($"/api/accounts/{result.Account.Id}", baseResponse);
        })
        .WithName("CreateAccount")
        .WithTags("Accounts")
        .Produces<BaseResponse<CreateAccountResponse>>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithSummary("Créer un compte de trésorerie")
        .WithDescription("Créer un nouveau compte de type CASH, BANK ou MOBILE_MONEY")
        .RequireAuthorization();
    }
}
