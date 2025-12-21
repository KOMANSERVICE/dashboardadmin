using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Repositories;
using Mapster;
using TresorerieService.Application.Features.PaymentMethods.DTOs;
using TresorerieService.Domain.Entities;

namespace TresorerieService.Application.Features.PaymentMethods.Commands.CreatePaymentMethod;

public class CreatePaymentMethodHandler(
        IGenericRepository<PaymentMethod> paymentMethodRepository,
        IUnitOfWork unitOfWork
    )
    : ICommandHandler<CreatePaymentMethodCommand, CreatePaymentMethodResult>
{
    public async Task<CreatePaymentMethodResult> Handle(
        CreatePaymentMethodCommand command,
        CancellationToken cancellationToken = default)
    {
        // Si la méthode est marquée comme défaut, retirer le défaut des autres méthodes
        if (command.IsDefault)
        {
            var existingDefaults = await paymentMethodRepository.GetByConditionAsync(
                pm => pm.ApplicationId == command.ApplicationId
                     && pm.BoutiqueId == command.BoutiqueId
                     && pm.IsDefault,
                cancellationToken);

            foreach (var existingDefault in existingDefaults)
            {
                existingDefault.IsDefault = false;
                paymentMethodRepository.UpdateData(existingDefault);
            }
        }

        // Créer la nouvelle méthode de paiement
        var paymentMethod = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            ApplicationId = command.ApplicationId,
            BoutiqueId = command.BoutiqueId,
            Name = command.Name,
            Type = command.Type,
            IsDefault = command.IsDefault,
            IsActive = true
        };

        await paymentMethodRepository.AddDataAsync(paymentMethod, cancellationToken);
        await unitOfWork.SaveChangesDataAsync(cancellationToken);

        var paymentMethodDto = paymentMethod.Adapt<PaymentMethodDTO>();

        return new CreatePaymentMethodResult(paymentMethodDto);
    }
}
