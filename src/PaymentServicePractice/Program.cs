using PaymentServicePractice.Gateways;
using PaymentServicePractice.Services;

// TEST 1: Lost first response and automatic retry
Console.WriteLine("=== Test 1: Automatic Retry ===");

var gateway = new FakePaymentGateway(
    loseFirstResponse: true);

var service = new PaymentService(
    new Dictionary<string, IPaymentGateway>
    {
        { "razorpay", gateway }
    });

var request = new PaymentRequest(
    "ORDER-101",
    150000,
    PaymentMethod.UPI,
    "razorpay");

var payment = service.Pay("PAY-101", request);

Console.WriteLine($"Status: {payment.Status}");
Console.WriteLine($"Gateway calls: {gateway.ChargeCallCount}");
Console.WriteLine($"Unique charges: {gateway.ChargeCount}");


// TEST 2: Idempotency
Console.WriteLine("\n=== Test 2: Idempotency ===");

var duplicate = service.Pay("PAY-101", request);

Console.WriteLine($"Duplicate status: {duplicate.Status}");
Console.WriteLine($"Gateway calls: {gateway.ChargeCallCount}");

try
{
    var changedRequest = request with
    {
        AmountMinor = 200000
    };

    service.Pay("PAY-101", changedRequest);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Conflict detected: {ex.Message}");
}


// TEST 3: All 3 attempts are Unknown
Console.WriteLine("\n=== Test 3: Retry Exhaustion ===");

var uncertainGateway = new FakePaymentGateway(
    lostResponseCount: 3);

var uncertainService = new PaymentService(
    new Dictionary<string, IPaymentGateway>
    {
        { "razorpay", uncertainGateway }
    });

var uncertainPayment = uncertainService.Pay(
    "PAY-102",
    new PaymentRequest(
        "ORDER-102",
        250000,
        PaymentMethod.Card,
        "razorpay"));

Console.WriteLine($"Status: {uncertainPayment.Status}");
Console.WriteLine($"Gateway calls: {uncertainGateway.ChargeCallCount}");
Console.WriteLine($"Unique charges: {uncertainGateway.ChargeCount}");

// TEST 4: Successful refund
Console.WriteLine("\n=== Test 4: Refund ===");

var refunded = service.Refund("PAY-101");

Console.WriteLine($"Refund status: {refunded.Status}");

// Repeat the same refund
var duplicateRefund = service.Refund("PAY-101");

Console.WriteLine($"Duplicate refund: {duplicateRefund.Status}");


// TEST 5: Reject refund for Processing payment
Console.WriteLine("\n=== Test 5: Invalid Refund ===");

try
{
    uncertainService.Refund("PAY-102");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Refund rejected: {ex.Message}");
}


// TEST 6: Reconcile an uncertain payment
Console.WriteLine("\n=== Test 6: Webhook Reconciliation ===");

var reconciled = uncertainService.Webhook(
    "EVT-101",
    "razorpay",
    "PAY-102",
    PaymentStatus.Success);

Console.WriteLine($"Reconciled status: {reconciled.Status}");


// TEST 7: Duplicate webhook
Console.WriteLine("\n=== Test 7: Duplicate Webhook ===");

var duplicateEvent = uncertainService.Webhook(
    "EVT-101",
    "razorpay",
    "PAY-102",
    PaymentStatus.Success);

Console.WriteLine($"Duplicate event status: {duplicateEvent.Status}");


// TEST 8: Late conflicting webhook
Console.WriteLine("\n=== Test 8: Late Webhook ===");

var lateEvent = uncertainService.Webhook(
    "EVT-102",
    "razorpay",
    "PAY-102",
    PaymentStatus.Failed);

Console.WriteLine($"Status after late event: {lateEvent.Status}");


// TEST 9: Gateway mismatch
Console.WriteLine("\n=== Test 9: Gateway Mismatch ===");

try
{
    uncertainService.Webhook(
        "EVT-103",
        "stripe",
        "PAY-102",
        PaymentStatus.Success);
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Webhook rejected: {ex.Message}");
}


Console.WriteLine("\n=== Test 10: Concurrent Payments ===");

var concurrentGateway = new FakePaymentGateway();

var concurrentService = new PaymentService(
    new Dictionary<string, IPaymentGateway>
    {
        { "razorpay", concurrentGateway }
    });

var concurrentRequest = new PaymentRequest(
    "ORDER-300",
    150000,
    PaymentMethod.UPI,
    "razorpay"
);

// Create 20 concurrent tasks
var tasks = Enumerable.Range(1, 20)
    .Select(i => Task.Run(() =>
    {
        return concurrentService.Pay(
            "PAY-300",
            concurrentRequest);
    }))
    .ToArray();

// Wait for all tasks to finish
var results = await Task.WhenAll(tasks);

Console.WriteLine($"Total requests: {results.Length}");

Console.WriteLine(
    $"Unique payment IDs: {results.Select(p => p.Id).Distinct().Count()}");

Console.WriteLine(
    $"Gateway calls: {concurrentGateway.ChargeCallCount}");

Console.WriteLine(
    $"Unique charges: {concurrentGateway.ChargeCount}");

Console.WriteLine(
    $"All successful: {results.All(p => p.Status == PaymentStatus.Success)}");
