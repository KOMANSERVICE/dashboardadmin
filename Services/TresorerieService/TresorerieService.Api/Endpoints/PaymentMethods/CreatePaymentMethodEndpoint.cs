using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.PaymentMethods.Commands.CreatePaymentMethod;
using TresorerieService.Application.Features.PaymentMethods.DTOs;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Api.Endpoints.PaymentMethods;

public record CreatePaymentMethodRequest(
    string Name,
    PaymentMethodType Type,
    bool IsDefault
);

public record CreatePaymentMethodResponse(PaymentMethodDTO PaymentMethod);

public class CreatePaymentMethodEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/payment-methods", async (
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
            [FromBody] CreatePaymentMethodRequest request,
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

            var command = new CreatePaymentMethodCommand(
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                Name: request.Name,
                Type: request.Type,
                IsDefault: request.IsDefault
            );

            var result = await sender.Send(command, cancellationToken);

            var response = new CreatePaymentMethodResponse(result.PaymentMethod);
            var baseResponse = ResponseFactory.Success(response, "Méthode de paiement créée avec succès", StatusCodes.Status201Created);

            return Results.Created($"/api/payment-methods/{result.PaymentMethod.Id}", baseResponse);
        })
        .WithName("CreatePaymentMethod")
        .WithTags("PaymentMethods")
        .Produces<BaseResponse<CreatePaymentMethodResponse>>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithSummary("Créer une méthode de paiement")
        .WithDescription("Créer une nouvelle méthode de paiement de type CASH, CARD, TRANSFER, CHECK ou MOBILE")
        .RequireAuthorization();
    }
}
