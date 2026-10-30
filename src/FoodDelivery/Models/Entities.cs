namespace FoodDelivery.Models;

public class Customer { }

public class Restaurant
{
    public string Name { get; set; } = "";

    public Menu Menu { get; set; } = new();

    public List<Order> Orders { get; } = new();

    public Order PlaceOrder(Cart cart)
    {
        Order order = new Order
        {
            Cart = cart
        };

        Orders.Add(order);

        return order;
    }

  public void AcceptOrder(Order order)
  {
    if (!Orders.Contains(order) ||
        order.Status != OrderStatus.Placed)
        throw new InvalidOperationException("Invalid order state.");

    order.Status = OrderStatus.Accepted;
  }

  public void RejectOrder(Order order)
{
    if (!Orders.Contains(order) ||
        order.Status != OrderStatus.Placed)
        throw new InvalidOperationException("Invalid order state.");

    order.Status = OrderStatus.Rejected;
}
}

public class Menu
{
    public List<MenuItem> Items { get; } = new();
}

public class MenuItem
{
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}

public class Location { }

public class OrderItem
{
    public MenuItem MenuItem { get; set; } = new();
    public int Quantity { get; set; }

    public decimal Total =>
        MenuItem.Price * Quantity;
}

public class Cart
{
    public List<OrderItem> Items { get; } = new();

    public decimal Total =>
        Items.Sum(item => item.Total);
}

public enum OrderStatus
{
    Placed,
    Accepted,
    Rejected,
    RiderAssigned
}

public class DeliveryPartner
{
    public string Id { get; set; } = "";

    public HashSet<string> RejectedOrderIds { get; } = new();
}

public class Order
{
    public string Id { get; } = Guid.NewGuid().ToString();

    public Cart Cart { get; set; } = new();

    public OrderStatus Status { get; set; } = OrderStatus.Placed;

    public DeliveryPartner? AssignedPartner { get; set; }
}

public class FoodDeliveryOrder
{
    public Restaurant Restaurant { get; set; } = new();
    public Order RestaurantOrder { get; set; } = new();
}