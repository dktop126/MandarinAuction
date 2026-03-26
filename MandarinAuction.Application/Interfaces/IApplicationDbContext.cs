using MandarinAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MandarinAuction.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Mandarin> Mandarins { get; }
    DbSet<Auction> Auctions { get; }
    DbSet<User> Users { get; }
    Task<int> SaveChangesAsync();
}