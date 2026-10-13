public sealed record PaymentRequest(
    string OrderId,
    long AmountMinor,
    PaymentMethod Method,
    string GatewayId
)
{
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(OrderId))
        {
            throw new ArgumentException("OrderId cannot be null or empty", nameof(OrderId));
        }
        if (AmountMinor <= 0)
        {
            throw new ArgumentException("AmountMinor must be greater than zero", nameof(AmountMinor));
        }
        if (string.IsNullOrWhiteSpace(GatewayId))
        {
            throw new ArgumentException("GatewayId cannot be null or empty", nameof(GatewayId));
        }
    }
}
    
