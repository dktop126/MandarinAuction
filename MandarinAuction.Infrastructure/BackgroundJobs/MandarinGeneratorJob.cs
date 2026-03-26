using MandarinAuction.Application.Interfaces;
using MandarinAuction.Domain.Entities;
using MandarinAuction.Domain.Enums;

namespace MandarinAuction.Infrastructure.BackgroundJobs;

public class MandarinGeneratorJob
{
    private readonly IApplicationDbContext _context;
    private readonly Random _random = new();
    
    public MandarinGeneratorJob(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task GenerateMandarinAsync()
    {
        var now = DateTime.UtcNow;
        var spoilTime = now.AddHours(24);

        var mandarin = new Mandarin
        {
            Name = $"Мандаринка №{_random.Next(1000, 9999)}",
            CreatedAt = now,
            SpoilAt = spoilTime,
            Status = MandarinStatus.Created
        };
        
        _context.Mandarins.Add(mandarin);

        var auction = new Auction(
            mandarinId: mandarin.Id,
            startingPrice: 100,
            endTime: spoilTime
        );
        
        _context.Auctions.Add(auction);

        await _context.SaveChangesAsync();
    }
}