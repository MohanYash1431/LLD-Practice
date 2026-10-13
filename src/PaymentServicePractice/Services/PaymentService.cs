using PaymentServicePractice.Gateways;

namespace PaymentServicePractice.Services;

public sealed class PaymentService
{
    private readonly Dictionary<string, IPaymentGateway> _gateways;

    private readonly Dictionary<string, Payment> _payments = new();

    private readonly object _lock = new();

    public PaymentService(
        IDictionary<string, IPaymentGateway> gateways)
    {
        ArgumentNullException.ThrowIfNull(gateways);

        _gateways = new Dictionary<string, IPaymentGateway>(gateways);
    }

    public Payment Pay(string key, PaymentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        request.Validate();

        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Payment key is required.");

        lock (_lock)
        {
            // Step 1: Find gateway
            if (!_gateways.TryGetValue(request.GatewayId, out var gateway)
                || gateway is null)
            {
                throw new ArgumentException("Invalid payment gateway.");
            }

            // Step 2: Check idempotency
            if (_payments.TryGetValue(key, out var existing))
            {
                if (existing.Request != request)
                    throw new InvalidOperationException(
                        "Idempotency conflict");

                return existing.Snapshot();
            }

            // Step 3: Create payment
            var payment = new Payment(key, request);

            // Step 4: Store before calling gateway
            _payments.Add(key, payment);

            // Step 5: Call gateway
            var result = gateway.Charge(key, request);

            // Step 6: Update status
            switch (result)
            {
                case GatewayChargeResult.Success:
                    payment.updateStatus(PaymentStatus.Success);
                    break;

                case GatewayChargeResult.Declined:
                    payment.updateStatus(PaymentStatus.Failed);
                    break;

                case GatewayChargeResult.Pending:
                    // Keep payment in Processing
                    break;
            }

            // Step 7: Return a safe copy
            return payment.Snapshot();
        }
    }
}