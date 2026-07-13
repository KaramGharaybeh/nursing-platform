using FluentValidation;
using MediatR;
using NursingPlatform.Application.Payments.DTOs;

namespace NursingPlatform.Application.Payments.Commands.StartMyPaymentCheckout;

public class StartMyPaymentCheckoutCommand : IRequest<PaymentCheckoutSessionDto>
{
    public Guid OrderId { get; set; }
    public StartPaymentCheckoutRequest Request { get; set; } = new();
}

public class StartMyPaymentCheckoutCommandValidator : AbstractValidator<StartMyPaymentCheckoutCommand>
{
    public StartMyPaymentCheckoutCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Request).NotNull();
        RuleFor(x => x.Request.IdempotencyKey)
            .MaximumLength(128)
            .When(x => x.Request is not null && x.Request.IdempotencyKey is not null);
    }
}
