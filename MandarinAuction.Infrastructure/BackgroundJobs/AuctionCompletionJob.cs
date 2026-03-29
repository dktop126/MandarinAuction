using MandarinAuction.Application.Interfaces;
using MandarinAuction.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MandarinAuction.Infrastructure.BackgroundJobs;

/// <summary>
/// Задача для обработки завершенных аукционов.
/// Отправляет уведомление победителю в аукционе.
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

    public async Task<int> ProcessFinishedAuctions()
    {
        var now = DateTime.UtcNow;
        var finishedAuctions = await _context.Auctions
            .Include(a => a.Mandarin)
            .Where(a => a.Status == AuctionStatus.Active && a.EndTime < now)
            .ToListAsync();

        if (finishedAuctions.Count == 0)
            return 0;

        foreach (var auction in finishedAuctions)
        {
            auction.Finish();

            if (auction.LastBidderId.HasValue)
            {
                var winner = await _context.Users.FindAsync(new object[] { auction.LastBidderId.Value });
                if (winner != null)
                {
                    await _emailService.SendWinReceiptNotificationAsync(
                        winner.Email,
                        auction.Id,
                        auction.CurrentPrice);
                }
            }
        }
        
        await  _context.SaveChangesAsync();
        return finishedAuctions.Count;
    }
}