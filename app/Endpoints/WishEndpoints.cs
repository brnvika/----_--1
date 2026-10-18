using Microsoft.EntityFrameworkCore;
using Wishlist.Api.Contracts;
using Wishlist.Api.Data;
using Wishlist.Api.Metrics;
using Wishlist.Api.Models;

namespace Wishlist.Api.Endpoints;

public static class WishEndpoints
{
    public static IEndpointRouteBuilder MapWishEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/wishes");

        group.MapGet(string.Empty, GetAllAsync);
        group.MapGet("{id:int}", GetByIdAsync);
        group.MapPost(string.Empty, CreateAsync);
        group.MapPut("{id:int}", UpdateAsync);
        group.MapDelete("{id:int}", DeleteAsync);

        return app;
    }

    public static IEndpointRouteBuilder MapHealthEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", HealthAsync);
        return app;
    }

    private static async Task<IResult> GetAllAsync(WishlistDbContext db, CancellationToken cancellationToken)
    {
        var wishes = await db.Wishes
            .AsNoTracking()
            .OrderByDescending(wish => wish.CreatedAt)
            .ToListAsync(cancellationToken);

        return Results.Ok(wishes.Select(ToResponse));
    }

    private static async Task<IResult> GetByIdAsync(int id, WishlistDbContext db, CancellationToken cancellationToken)
    {
        var wish = await db.Wishes
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        return wish is null
            ? Results.NotFound(new { message = $"Желание с id={id} не найдено." })
            : Results.Ok(ToResponse(wish));
    }

    private static async Task<IResult> CreateAsync(
        CreateWishRequest request,
        WishlistDbContext db,
        CancellationToken cancellationToken)
    {
        var validationError = Validate(request.Title, request.Link, request.Price);
        if (validationError is not null)
        {
            return validationError;
        }

        var wish = new Wish
        {
            Title = request.Title.Trim(),
            Link = request.Link.Trim(),
            Price = request.Price,
            IsBought = request.IsBought,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.Wishes.Add(wish);
        await db.SaveChangesAsync(cancellationToken);

        WishMetrics.IncrementCreated();
        await WishMetrics.RefreshAsync(db, cancellationToken);

        return Results.Created($"/wishes/{wish.Id}", ToResponse(wish));
    }

    private static async Task<IResult> UpdateAsync(
        int id,
        UpdateWishRequest request,
        WishlistDbContext db,
        CancellationToken cancellationToken)
    {
        var validationError = Validate(request.Title, request.Link, request.Price);
        if (validationError is not null)
        {
            return validationError;
        }

        var wish = await db.Wishes.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (wish is null)
        {
            return Results.NotFound(new { message = $"Желание с id={id} не найдено." });
        }

        wish.Title = request.Title.Trim();
        wish.Link = request.Link.Trim();
        wish.Price = request.Price;
        wish.IsBought = request.IsBought;

        await db.SaveChangesAsync(cancellationToken);
        await WishMetrics.RefreshAsync(db, cancellationToken);

        return Results.Ok(ToResponse(wish));
    }

    private static async Task<IResult> DeleteAsync(int id, WishlistDbContext db, CancellationToken cancellationToken)
    {
        var wish = await db.Wishes.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (wish is null)
        {
            return Results.NotFound(new { message = $"Желание с id={id} не найдено." });
        }

        db.Wishes.Remove(wish);
        await db.SaveChangesAsync(cancellationToken);
        await WishMetrics.RefreshAsync(db, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> HealthAsync(WishlistDbContext db, CancellationToken cancellationToken)
    {
        var databaseReady = await db.Database.CanConnectAsync(cancellationToken);
        return databaseReady
            ? Results.Ok(new { status = "ok" })
            : Results.Json(new { status = "error", reason = "database unavailable" }, statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    private static IResult? Validate(string title, string link, decimal price)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(title))
        {
            errors["title"] = ["Название обязательно."];
        }

        if (string.IsNullOrWhiteSpace(link))
        {
            errors["link"] = ["Ссылка обязательна."];
        }
        else if (!Uri.TryCreate(link, UriKind.Absolute, out _))
        {
            errors["link"] = ["Ссылка должна быть абсолютным URL."];
        }

        if (price < 0)
        {
            errors["price"] = ["Цена не может быть отрицательной."];
        }

        return errors.Count == 0 ? null : Results.ValidationProblem(errors);
    }

    private static WishResponse ToResponse(Wish wish) =>
        new(wish.Id, wish.Title, wish.Link, wish.Price, wish.IsBought, wish.CreatedAt);
}
