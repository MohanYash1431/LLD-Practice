using FoodDelivery.Models;

namespace FoodDelivery.Services;

public class RestaurantManager
{
    private readonly List<Restaurant> _restaurants = new();

    public void AddRestaurant(Restaurant restaurant)
    {
        _restaurants.Add(restaurant);
    }

    public Restaurant? FindRestaurant(string searchQuery)
    {
        return _restaurants.FirstOrDefault(r =>
            r.Name.Contains(
                searchQuery,
                StringComparison.OrdinalIgnoreCase));
    }
}