using FoodDelivery.Models;

namespace FoodDelivery.Services;

public class FoodDeliveryApp
{
    private readonly RestaurantManager _restaurants;
    private readonly DeliveryService _delivery;
    private readonly PaymentService _payments;
    private readonly NotificationService _notifications;

    public FoodDeliveryApp(
        RestaurantManager restaurants,
        DeliveryService delivery,
        PaymentService payments,
        NotificationService notifications)
    {
        _restaurants = restaurants;
        _delivery = delivery;
        _payments = payments;
        _notifications = notifications;
    }

    public Restaurant? SearchRestaurant(string query)
        => _restaurants.FindRestaurant(query);

    public FoodDeliveryOrder PlaceOrder(Restaurant restaurant, Cart cart)
    {
        Order order = restaurant.PlaceOrder(cart);

        FoodDeliveryOrder foodOrder = new()
        {
            Restaurant = restaurant,
            RestaurantOrder = order
        };

        _payments.RequestPayment(foodOrder);
        _notifications.Send("Order placed");

        return foodOrder;
    }

    public DeliveryPartner? AssignDelivery(
        FoodDeliveryOrder foodOrder,
        IEnumerable<DeliveryPartner> nearbyRiders,
        Func<DeliveryPartner, bool> acceptsOffer)
    {
        return _delivery.AssignPartner(
            foodOrder.RestaurantOrder,
            nearbyRiders,
            acceptsOffer);
    }
}