namespace Wishlist.Api.Contracts;

public sealed record WishResponse(
    int Id,
    string Title,
    string Link,
    decimal Price,
    bool IsBought,
    DateTimeOffset CreatedAt);

public sealed record CreateWishRequest(
    string Title,
    string Link,
    decimal Price,
    bool IsBought = false);

public sealed record UpdateWishRequest(
    string Title,
    string Link,
    decimal Price,
    bool IsBought);
