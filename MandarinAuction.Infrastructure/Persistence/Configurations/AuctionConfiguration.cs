using MandarinAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MandarinAuction.Infrastructure.Persistence.Configurations;

/// <summary>
/// Конфигурация сущности Auction для Entity Framework.
/// Определяет первичный ключ и версию строки для оптимистичной блокировки.
/// </summary>
public class AuctionConfiguration : IEntityTypeConfiguration<Auction>
{
    public void Configure(EntityTypeBuilder<Auction> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.RowVersion).IsRowVersion();
    }
}