using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.Accounts.Queries.GetAccountDetail;

namespace TresorerieService.Api.Endpoints.Accounts;

public class GetAccountDetailEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/accounts/{id:guid}", async (
            [FromRoute] Guid id,
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
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

            var query = new GetAccountDetailQuery(
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                AccountId: id,
                FromDate: fromDate,
                ToDate: toDate
            );

            var result = await sender.Send(query, cancellationToken);

            var response = ResponseFactory.Success(result, "Detail du compte recupere avec succes");

            return Results.Ok(response);
        })
        .WithName("GetAccountDetail")
        .WithTags("Accounts")
        .Produces<BaseResponse<GetAccountDetailResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Consulter le detail d'un compte")
        .WithDescription("Recupere le detail complet d'un compte de tresorerie incluant les totaux des entrees/sorties et les 20 derniers mouvements. Optionnellement, inclut l'evolution du solde sur une periode si fromDate et toDate sont specifies.")
        .RequireAuthorization();
    }
}
