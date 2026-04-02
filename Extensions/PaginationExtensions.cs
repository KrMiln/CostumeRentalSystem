using CostumeRentalSystem.Common;
using CostumeRentalSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

public static class PaginationExtensions
{

    public static async Task<PagedResult<T>> ToPagedResultAsync<T>
        (this IQueryable<T> query, int page, int pageSize)
    {
        var totalItems = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize)
            .Take(pageSize).ToListAsync();

        return new PagedResult<T>
        {
            Items = items,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            TotalItems = totalItems
        };
    }

    public static Dictionary<string, string?> ToRouteValues(this object model)
    {
        var routes = new Dictionary<string, string?>();

        var properties = model.GetType().GetProperties();

        foreach (var prop in properties)
        {
            if (prop.Name == "Rentals" || prop.Name == "Costumes" || prop.Name == "Pagination" || prop.Name == "Categories")
                continue;

            var value = prop.GetValue(model);

            if (value != null)
            {
                if (value is DateTime dt)
                {
                    routes[prop.Name] = dt.ToString("yyyy-MM-dd");
                }
                else
                {
                    routes[prop.Name] = value.ToString();
                }
            }
        }

        return routes;
    }
}