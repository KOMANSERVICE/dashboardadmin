using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.Accounts.DTOs;
using TresorerieService.Application.Features.Accounts.Queries.GetAccountBalance;

namespace TresorerieService.Api.Endpoints.Accounts;

public class GetAccountBalanceEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/accounts/{id:guid}/balance", async (
            [FromRoute] Guid id,
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
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

            var query = new GetAccountBalanceQuery(
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                AccountId: id
            );

            var result = await sender.Send(query, cancellationToken);

            var response = ResponseFactory.Success(result, "Solde du compte recupere avec succes");

            return Results.Ok(response);
        })
        .WithName("GetAccountBalance")
        .WithTags("Accounts")
        .Produces<BaseResponse<AccountBalanceDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Consulter le solde en temps reel d'un compte")
        .WithDescription("Recupere le solde actuel d'un compte de tresorerie avec les variations depuis le debut du mois et de la journee. Une alerte est indiquee si le solde est inferieur au seuil configure.")
        .RequireAuthorization();
    }
}
