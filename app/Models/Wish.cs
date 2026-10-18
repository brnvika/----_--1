using System.ComponentModel.DataAnnotations;

namespace Wishlist.Api.Models;

public sealed class Wish
{
    public int Id { get; set; }

    [MaxLength(200)]
    public required string Title { get; set; }

    [MaxLength(2048)]
    public required string Link { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    public bool IsBought { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
