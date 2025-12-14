using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.Accounts.Queries.GetAccounts;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Api.Endpoints.Accounts;

public class GetAccountsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/accounts", async (
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
            [FromQuery] bool includeInactive = false,
            [FromQuery] AccountType? type = null,
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

            var query = new GetAccountsQuery(
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                IncludeInactive: includeInactive,
                Type: type
            );

            var result = await sender.Send(query, cancellationToken);

            var response = ResponseFactory.Success(result, "Liste des comptes recuperee avec succes");

            return Results.Ok(response);
        })
        .WithName("GetAccounts")
        .WithTags("Accounts")
        .Produces<BaseResponse<GetAccountsResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithSummary("Lister les comptes de tresorerie")
        .WithDescription("Recupere la liste des comptes avec filtres optionnels par type et statut actif/inactif")
        .RequireAuthorization();
    }
}
