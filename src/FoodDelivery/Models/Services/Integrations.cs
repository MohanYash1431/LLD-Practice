using FoodDelivery.Models;

namespace FoodDelivery.Services;

public class PaymentService
{
    public void RequestPayment(FoodDeliveryOrder order)
    {
        Console.WriteLine(
            $"[Demo] Payment requested: ₹{order.RestaurantOrder.Cart.Total}");
    }
}

public class NotificationService
{
    public void Send(string message)
    {
        Console.WriteLine($"[Notification] {message}");
    }
}