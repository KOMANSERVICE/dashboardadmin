using IDR.Library.BuildingBlocks.CQRS;
using TresorerieService.Application.Features.PaymentMethods.DTOs;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Application.Features.PaymentMethods.Commands.CreatePaymentMethod;

public record CreatePaymentMethodCommand(
    string ApplicationId,
    string BoutiqueId,
    string Name,
    PaymentMethodType Type,
    bool IsDefault
) : ICommand<CreatePaymentMethodResult>;

public record CreatePaymentMethodResult(PaymentMethodDTO PaymentMethod);
