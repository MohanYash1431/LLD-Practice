using PaymentServicePractice.Gateways;
using PaymentServicePractice.Services;

var gateway = new FakePaymentGateway(
    loseFirstResponse: true);

var gateways = new Dictionary<string, IPaymentGateway>
{
    { "razorpay", gateway }
};

var service = new PaymentService(gateways);

var request = new PaymentRequest(
    "ORDER-101",
    150000,
    PaymentMethod.UPI,
    "razorpay"
);

// First payment
var payment = service.Pay("PAY-101", request);

Console.WriteLine($"Payment ID: {payment.Id}");
Console.WriteLine($"Payment Status: {payment.Status}");

// Duplicate request using the same key
var duplicate = service.Pay("PAY-101", request);

Console.WriteLine($"Duplicate Status: {duplicate.Status}");
Console.WriteLine($"Gateway Charges: {gateway.ChargeCount}");