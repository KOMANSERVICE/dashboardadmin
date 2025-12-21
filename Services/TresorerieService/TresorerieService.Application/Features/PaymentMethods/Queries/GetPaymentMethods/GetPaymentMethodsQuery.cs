using IDR.Library.BuildingBlocks.CQRS;

namespace TresorerieService.Application.Features.PaymentMethods.Queries.GetPaymentMethods;

/// <summary>
/// Query pour récupérer la liste des méthodes de paiement
/// </summary>
public record GetPaymentMethodsQuery(
    string ApplicationId,
    string BoutiqueId
) : IQuery<GetPaymentMethodsResponse>;
