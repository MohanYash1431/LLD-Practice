public interface IPaymentGateway
{
    GatewayChargeResult Charge(string paymentId, PaymentRequest request);

    bool Refund(string paymentId);
}