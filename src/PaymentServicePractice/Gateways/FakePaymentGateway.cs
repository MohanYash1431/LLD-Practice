namespace PaymentServicePractice.Gateways;

public sealed class FakePaymentGateway : IPaymentGateway
{
    private readonly HashSet<string> _charged = new();
    private readonly HashSet<string> _refunded = new();

    private bool _loseFirstResponse;
    private readonly bool _decline;

    private readonly object _lock = new();

    public FakePaymentGateway(
        bool loseFirstResponse = false,
        bool decline = false)
    {
        _loseFirstResponse = loseFirstResponse;
        _decline = decline;
    }

    public GatewayChargeResult Charge(
        string paymentId,
        PaymentRequest request)
    {
        lock (_lock)
        {
            if (_decline)
                return GatewayChargeResult.Declined;

            // Same ID will not create another charge.
            _charged.Add(paymentId);

            // Simulate a lost gateway response.
            if (_loseFirstResponse)
            {
                _loseFirstResponse = false;
                return GatewayChargeResult.Pending;
            }

            return GatewayChargeResult.Success;
        }
    }

    public bool Refund(string paymentId)
    {
        lock (_lock)
        {
            if (!_charged.Contains(paymentId))
                return false;

            _refunded.Add(paymentId);
            return true;
        }
    }

    public int ChargeCount
    {
        get
        {
            lock (_lock)
            {
                return _charged.Count;
            }
        }
    }
}