using MandarinAuction.Application.Interfaces;
using MandarinAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MandarinAuction.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Mandarin> Mandarins { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Auction> Auctions { get; set; }
    public DbSet<Bid> Bids { get; set; }

    Task IApplicationDbContext.SaveChangesAsync()
    {
        return SaveChangesAsync();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Mandarin>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Status).IsRequired();
        });

        modelBuilder.Entity<Auction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StartingPrice).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.CurrentPrice).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<Bid>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).IsRequired().HasPrecision(18, 2);

            entity.HasOne(d => d.Auction)
                .WithMany()
                .HasForeignKey(d => d.AuctionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.User)
                .WithMany(p => p.Bids)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}