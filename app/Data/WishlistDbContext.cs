using Microsoft.EntityFrameworkCore;
using Wishlist.Api.Models;

namespace Wishlist.Api.Data;

public sealed class WishlistDbContext : DbContext
{
    public WishlistDbContext(DbContextOptions<WishlistDbContext> options)
        : base(options)
    {
    }

    public DbSet<Wish> Wishes => Set<Wish>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var wish = modelBuilder.Entity<Wish>();

        wish.ToTable("wishes");
        wish.HasKey(x => x.Id);

        wish.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        wish.Property(x => x.Link)
            .IsRequired()
            .HasMaxLength(2048);

        wish.Property(x => x.Price)
            .HasPrecision(18, 2);

        wish.Property(x => x.IsBought)
            .HasDefaultValue(false);

        wish.Property(x => x.CreatedAt)
            .HasDefaultValueSql("timezone('utc', now())");

        wish.HasIndex(x => x.IsBought);
    }
}
