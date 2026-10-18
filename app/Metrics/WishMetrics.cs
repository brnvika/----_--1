using Microsoft.EntityFrameworkCore;
using Prometheus;
using Wishlist.Api.Data;

namespace Wishlist.Api.Metrics;

public static class WishMetrics
{
    // Счётчик создаваемых желаний.
    private static readonly Counter WishesCreated = global::Prometheus.Metrics
        .CreateCounter("wishes_created_total", "Общее число созданных желаний.");

    // Gauge показывает текущее количество желаний, которые ещё не куплены.
    private static readonly Gauge WishesActive = global::Prometheus.Metrics
        .CreateGauge("wishes_active", "Количество желаний, которые ещё не отмечены как купленные.");

    public static void IncrementCreated()
    {
        WishesCreated.Inc();
    }

    public static async Task RefreshAsync(WishlistDbContext db, CancellationToken cancellationToken = default)
    {
        var activeCount = await db.Wishes.CountAsync(wish => !wish.IsBought, cancellationToken);
        WishesActive.Set(activeCount);
    }
}
