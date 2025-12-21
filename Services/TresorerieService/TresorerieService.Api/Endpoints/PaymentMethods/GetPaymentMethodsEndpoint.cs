using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.PaymentMethods.Queries.GetPaymentMethods;

namespace TresorerieService.Api.Endpoints.PaymentMethods;

public class GetPaymentMethodsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/payment-methods", async (
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

            var query = new GetPaymentMethodsQuery(
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId
            );

            var result = await sender.Send(query, cancellationToken);

            var response = ResponseFactory.Success(result, "Liste des methodes de paiement recuperee avec succes");

            return Results.Ok(response);
        })
        .WithName("GetPaymentMethods")
        .WithTags("PaymentMethods")
        .Produces<BaseResponse<GetPaymentMethodsResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithSummary("Lister les methodes de paiement")
        .WithDescription("Recupere la liste des methodes de paiement actives de la boutique, triees par nom. La methode par defaut est identifiee par le champ IsDefault.")
        .RequireAuthorization();
    }
}
