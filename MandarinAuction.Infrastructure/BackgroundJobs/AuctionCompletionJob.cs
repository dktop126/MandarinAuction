using MandarinAuction.Application.Interfaces;
using MandarinAuction.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MandarinAuction.Infrastructure.BackgroundJobs;

/// <summary>
/// Задача для обработки завершенных аукционов и испорченных мандаринов.
/// Выполняется ежедневно в 00:00 UTC.
/// Сначала завершает аукционы с победителями, затем помечает оставшиеся мандарины как испорченные.
/// </summary>
public class AuctionCompletionJob
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public AuctionCompletionJob(IApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<(int spoiled, int finished)> ProcessDailyAuctionCleanup()
    {
        var now = DateTime.UtcNow;
        
        var finishedAuctions = await _context.Auctions
            .Include(a => a.Mandarin)
            .Where(a => a.Status == AuctionStatus.Active && 
                        a.EndTime <= now &&
                        a.LastBidderId.HasValue)
            .ToListAsync();

        foreach (var auction in finishedAuctions)
        {
            auction.Finish();
            auction.Mandarin?.MarkAsSold();
            
            var winner = await _context.Users.FindAsync(new object[] { auction.LastBidderId.Value });
            if (winner != null)
            {
                await _emailService.SendWinReceiptNotificationAsync(
                    winner.Email,
                    auction.Id,
                    auction.CurrentPrice);
            }
        }
        
        var expiredMandarins = await _context.Mandarins
            .Include(m => m.Auction)
            .Where(m => m.Status != MandarinStatus.Sold &&
                        m.Status != MandarinStatus.Spoiled &&
                        m.SpoilAt <= now)
            .ToListAsync();

        foreach (var mandarin in expiredMandarins)
        {
            mandarin.MarkAsSpoiled(now);
            mandarin.Auction?.CloseAsSpoiled();
        }
        
        await _context.SaveChangesAsync();
        return (expiredMandarins.Count, finishedAuctions.Count);
    }
}