using FoodDelivery.Models;
using FoodDelivery.Services;

// Setup
Restaurant restaurant = new() { Name = "Pizza House" };

RestaurantManager manager = new();
manager.AddRestaurant(restaurant);

FoodDeliveryApp app = new(
    manager,
    new DeliveryService(),
    new PaymentService(),
    new NotificationService());

restaurant.Menu.Items.Add(new MenuItem
{
    Name = "Pizza",
    Price = 200
});

// Helper: new cart for each order
Cart CreateCart()
{
    Cart cart = new();

    cart.Items.Add(new OrderItem
    {
        MenuItem = restaurant.Menu.Items[0],
        Quantity = 2
    });

    return cart;
}

// TEST 1: Restaurant rejects
var rejected = app.PlaceOrder(restaurant, CreateCart());

restaurant.RejectOrder(rejected.RestaurantOrder);

var noRider = app.AssignDelivery(
    rejected,
    new[] { new DeliveryPartner { Id = "D1" } },
    _ => true);

Console.WriteLine($"Rejected order rider: {noRider?.Id ?? "None"}");

// TEST 2: Restaurant accepts; D1 rejects, D2 accepts
var accepted = app.PlaceOrder(restaurant, CreateCart());

restaurant.AcceptOrder(accepted.RestaurantOrder);

var d1 = new DeliveryPartner { Id = "D1" };
var d2 = new DeliveryPartner { Id = "D2" };

var rider = app.AssignDelivery(
    accepted,
    new[] { d1, d2 },
    partner => partner.Id == "D2");

Console.WriteLine($"Assigned rider: {rider?.Id ?? "None"}");

// TEST 3: Two concurrent orders compete for D3
var orderA = app.PlaceOrder(restaurant, CreateCart());
var orderB = app.PlaceOrder(restaurant, CreateCart());

restaurant.AcceptOrder(orderA.RestaurantOrder);
restaurant.AcceptOrder(orderB.RestaurantOrder);

var d3 = new DeliveryPartner { Id = "D3" };
int assignments = 0;

void TryAssign(FoodDeliveryOrder order)
{
    var result = app.AssignDelivery(
        order,
        new[] { d3 },
        _ => true);

    if (result is not null)
        Interlocked.Increment(ref assignments);
}

Parallel.Invoke(
    () => TryAssign(orderA),
    () => TryAssign(orderB));

Console.WriteLine($"D3 assignments: {assignments}");