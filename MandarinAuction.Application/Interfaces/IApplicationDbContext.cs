using MandarinAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MandarinAuction.Application.Interfaces;

/// <summary>
/// Интерфейс контекста базы данных для доступа к сущностям.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Mandarin> Mandarins { get; }
    DbSet<Auction> Auctions { get; }
    DbSet<User> Users { get; }
    DbSet<Bid> Bids { get; }
    Task SaveChangesAsync();
}