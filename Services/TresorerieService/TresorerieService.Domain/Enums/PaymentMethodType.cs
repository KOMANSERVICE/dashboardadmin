namespace TresorerieService.Domain.Enums;

/// <summary>
/// Types de méthodes de paiement disponibles
/// </summary>
public enum PaymentMethodType
{
    /// <summary>
    /// Paiement en espèces
    /// </summary>
    CASH = 1,

    /// <summary>
    /// Paiement par carte bancaire
    /// </summary>
    CARD = 2,

    /// <summary>
    /// Paiement par virement bancaire
    /// </summary>
    TRANSFER = 3,

    /// <summary>
    /// Paiement par chèque
    /// </summary>
    CHECK = 4,

    /// <summary>
    /// Paiement par mobile money
    /// </summary>
    MOBILE = 5
}
