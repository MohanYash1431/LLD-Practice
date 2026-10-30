using FoodDelivery.Models;

namespace FoodDelivery.Services;

public class DeliveryService
{
    private readonly object _lock = new();

    private readonly HashSet<string> _busyRiders = new();

    public DeliveryPartner? AssignPartner(
        Order order,
        IEnumerable<DeliveryPartner> nearbyRiders,
        Func<DeliveryPartner, bool> acceptsOffer)
    {
        lock (_lock)
        {
            if (order.Status == OrderStatus.RiderAssigned)
                return order.AssignedPartner;

            if (order.Status != OrderStatus.Accepted)
                return null;

            foreach (var rider in nearbyRiders)
            {
                if (_busyRiders.Contains(rider.Id) ||
                    rider.RejectedOrderIds.Contains(order.Id))
                    continue;

                if (!acceptsOffer(rider))
                {
                    rider.RejectedOrderIds.Add(order.Id);
                    continue;
                }

                _busyRiders.Add(rider.Id);
                order.AssignedPartner = rider;
                order.Status = OrderStatus.RiderAssigned;

                return rider;
            }

            return null;
        }
    }
}