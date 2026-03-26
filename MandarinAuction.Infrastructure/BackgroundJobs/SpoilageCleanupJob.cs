using MandarinAuction.Application.Interfaces;
using MandarinAuction.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MandarinAuction.Infrastructure.BackgroundJobs;

public class SpoilageCleanupJob
{
    private readonly IApplicationDbContext _context;
    
    public SpoilageCleanupJob(IApplicationDbContext context) => _context = context;

    public async Task CleanupSpoiledMandarins()
    {
        var now = DateTime.UtcNow;
        var expiredMandarins = await _context.Mandarins
            .Where(m => m.Status != MandarinStatus.Sold &&
                        m.Status == MandarinStatus.Spoiled &&
                        m.SpoilAt < now).ToListAsync();

        foreach (var mandarin in expiredMandarins)
        {
            mandarin.MarkAsSpoiled(now);

            var activeAuctions = await _context.Auctions
                .FirstOrDefaultAsync(a => a.MandarinId == mandarin.Id && a.Status == AuctionStatus.Active);
            activeAuctions?.CloseAsSpoiled();
        }
        
        await _context.SaveChangesAsync();
    }
}