using Microsoft.EntityFrameworkCore;
using Prometheus;
using Wishlist.Api.Data;
using Wishlist.Api.Endpoints;
using Wishlist.Api.Metrics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<WishlistDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("WishlistDb")
        ?? throw new InvalidOperationException("Строка подключения WishlistDb не задана.");

    options.UseNpgsql(connectionString);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WishlistDbContext>();
    // Создаём таблицы при старте, если схема ещё не развёрнута.
    await db.Database.EnsureCreatedAsync();
    // Сразу пересчитываем число активных желаний для метрики wishes_active.
    await WishMetrics.RefreshAsync(db);
}

app.UseHttpMetrics();
app.UseMetricServer();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapWishEndpoints();
app.MapHealthEndpoint();

app.Run();
