namespace NursingPlatform.Application.Payments.Commands.StartMyPaymentCheckout;

public class StartPaymentCheckoutRequest
{
    public string? IdempotencyKey { get; set; }
}
