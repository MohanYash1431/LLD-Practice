namespace PaymentServicePractice.Gateways;

public sealed class FakePaymentGateway : IPaymentGateway
{
    private readonly HashSet<string> _charged = new();
    private readonly HashSet<string> _refunded = new();

    private int _lostResponsesRemaining;
    private int _chargeCallCount;

    private readonly bool _decline;
    private readonly object _lock = new();

    public FakePaymentGateway(
        bool loseFirstResponse = false,
        bool decline = false,
        int lostResponseCount = 0)
    {
        if (lostResponseCount < 0)
            throw new ArgumentOutOfRangeException(
                nameof(lostResponseCount));

        _lostResponsesRemaining = Math.Max(
            lostResponseCount,
            loseFirstResponse ? 1 : 0);

        _decline = decline;
    }

    public GatewayChargeResult Charge(
        string paymentId,
        PaymentRequest request)
    {
        lock (_lock)
        {
            _chargeCallCount++;

            if (_decline)
                return GatewayChargeResult.Declined;

            // Prevent duplicate charges
            _charged.Add(paymentId);

            // Simulate lost gateway responses
            if (_lostResponsesRemaining > 0)
            {
                _lostResponsesRemaining--;
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
                return _charged.Count;
        }
    }

    public int ChargeCallCount
    {
        get
        {
            lock (_lock)
                return _chargeCallCount;
        }
    }
}
