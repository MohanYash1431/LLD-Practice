using PaymentServicePractice.Gateways;

namespace PaymentServicePractice.Services;

public sealed class PaymentService
{
    private readonly Dictionary<string, IPaymentGateway> _gateways;

    private readonly Dictionary<string, Payment> _payments = new();

    private readonly HashSet<(string GatewayId, string EventId)>
    _processedWebhookEvents = new();

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
        // Step 1: Find registered gateway
        if (!_gateways.TryGetValue(request.GatewayId, out var gateway)
            || gateway is null)
        {
            throw new ArgumentException("Invalid payment gateway.");
        }

        // Step 2: Idempotency check
        if (_payments.TryGetValue(key, out var existing))
        {
            if (existing.Request != request)
            {
                throw new InvalidOperationException(
                    "Idempotency conflict");
            }

            return existing.Snapshot();
        }

        // Step 3: Create and store payment
        var payment = new Payment(key, request);
        _payments.Add(key, payment);

        // Step 4: Retry up to 3 total attempts
        const int maxAttempts = 3;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            Console.WriteLine(
                $"Gateway attempt {attempt} for {payment.Id}");

            var result = gateway.Charge(payment.Id, request);

            switch (result)
            {
                case GatewayChargeResult.Success:
                    payment.updateStatus(PaymentStatus.Success);
                    return payment.Snapshot();

                case GatewayChargeResult.Declined:
                    payment.updateStatus(PaymentStatus.Failed);
                    return payment.Snapshot();

                case GatewayChargeResult.Pending:
                    // Retry using the SAME payment ID.
                    break;

                default:
                    throw new InvalidOperationException(
                        "Unsupported gateway result.");
            }
        }

        // All attempts returned Unknown.
        // Do not assume payment failed.
        return payment.Snapshot();
    }
}


public Payment Refund(string key)
{
    if (string.IsNullOrWhiteSpace(key))
        throw new ArgumentException("Payment key is required.");

    lock (_lock)
    {
        // Step 1: Find payment
        if (!_payments.TryGetValue(key, out var payment))
            throw new ArgumentException("Unknown payment.");

        // Step 2: Already refunded? Return existing result
        if (payment.Status == PaymentStatus.Refunded)
            return payment.Snapshot();

        // Step 3: Only successful payments can be refunded
        if (payment.Status != PaymentStatus.Success)
            throw new InvalidOperationException(
                "Only successful payments can be refunded.");

        // Step 4: Find original gateway
        var gateway = _gateways[payment.Request.GatewayId];

        // Step 5: Request refund using the original payment ID
        bool refundSuccess = gateway.Refund(payment.Id);

        if (!refundSuccess)
            throw new InvalidOperationException(
                "Refund not confirmed. Retry with the same payment ID.");

        // Step 6: Update payment status
        payment.updateStatus(PaymentStatus.Refunded);

        return payment.Snapshot();
    }
}


public Payment Webhook(
    string eventId,
    string gatewayId,
    string key,
    PaymentStatus status)
{
    // Step 1: Validate event information
    if (string.IsNullOrWhiteSpace(eventId) ||
        string.IsNullOrWhiteSpace(gatewayId) ||
        string.IsNullOrWhiteSpace(key))
    {
        throw new ArgumentException(
            "Invalid webhook information.");
    }

    if (status != PaymentStatus.Success &&
        status != PaymentStatus.Failed)
    {
        throw new ArgumentException(
            "Invalid webhook status.");
    }

    lock (_lock)
    {
        // Step 2: Find the original payment
        if (!_payments.TryGetValue(key, out var payment))
        {
            throw new ArgumentException("Unknown payment.");
        }

        // Step 3: Validate gateway identity
        if (payment.Request.GatewayId != gatewayId)
        {
            throw new ArgumentException(
                "Webhook gateway does not match payment gateway.");
        }

        // Step 4: Deduplicate the webhook event
        if (!_processedWebhookEvents.Add((gatewayId, eventId)))
        {
            return payment.Snapshot();
        }

        // Step 5: Reconcile only Processing payments
        if (payment.Status == PaymentStatus.Processing)
        {
            payment.updateStatus(status);
        }

        // Step 6: Return an independent snapshot
        return payment.Snapshot();
    }
}



}