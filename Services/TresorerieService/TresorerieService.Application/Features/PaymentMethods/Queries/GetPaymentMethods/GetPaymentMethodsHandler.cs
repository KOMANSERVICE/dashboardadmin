using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Repositories;
using Mapster;
using TresorerieService.Application.Features.PaymentMethods.DTOs;
using TresorerieService.Domain.Entities;

namespace TresorerieService.Application.Features.PaymentMethods.Queries.GetPaymentMethods;

/// <summary>
/// Handler pour récupérer la liste des méthodes de paiement actives d'une boutique
/// </summary>
public class GetPaymentMethodsHandler(IGenericRepository<PaymentMethod> paymentMethodRepository)
    : IQueryHandler<GetPaymentMethodsQuery, GetPaymentMethodsResponse>
{
    public async Task<GetPaymentMethodsResponse> Handle(
        GetPaymentMethodsQuery query,
        CancellationToken cancellationToken = default)
    {
        // Récupérer uniquement les méthodes de paiement actives de la boutique
        var paymentMethods = await paymentMethodRepository.GetByConditionAsync(
            pm => pm.ApplicationId == query.ApplicationId
                  && pm.BoutiqueId == query.BoutiqueId
                  && pm.IsActive,
            cancellationToken);

        // Trier par nom et mapper vers les DTOs
        var paymentMethodDtos = paymentMethods
            .OrderBy(pm => pm.Name)
            .Select(pm => pm.Adapt<PaymentMethodDTO>())
            .ToList();

        return new GetPaymentMethodsResponse(
            PaymentMethods: paymentMethodDtos,
            TotalCount: paymentMethodDtos.Count
        );
    }
}
