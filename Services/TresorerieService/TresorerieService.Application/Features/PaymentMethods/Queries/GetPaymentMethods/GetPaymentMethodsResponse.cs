using TresorerieService.Application.Features.PaymentMethods.DTOs;

namespace TresorerieService.Application.Features.PaymentMethods.Queries.GetPaymentMethods;

/// <summary>
/// Réponse pour la liste des méthodes de paiement
/// </summary>
public record GetPaymentMethodsResponse(
    IReadOnlyList<PaymentMethodDTO> PaymentMethods,
    int TotalCount
);
