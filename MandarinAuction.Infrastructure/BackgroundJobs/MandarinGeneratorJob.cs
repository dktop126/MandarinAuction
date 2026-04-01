using MandarinAuction.Application.Interfaces;
using MandarinAuction.Domain.Entities;
using MandarinAuction.Domain.Enums;

namespace MandarinAuction.Infrastructure.BackgroundJobs;

/// <summary>
/// Задача для генерации новых мандаринов и создания для них аукционов.
/// </summary>
public class MandarinGeneratorJob
{
    private readonly IApplicationDbContext _context;
    private readonly Random _random = new();
    
    private static readonly string[] MandarinImages = new[]
    {
        "https://images.unsplash.com/photo-1611080626919-7cf5a9dbab5b?w=400&h=400&fit=crop",
        "https://images.unsplash.com/photo-1557800636-894a64c1696f?w=400&h=400&fit=crop",
        "https://images.unsplash.com/photo-1580052614034-c55d20bfee3b?w=400&h=400&fit=crop",
        "https://images.unsplash.com/photo-1611080626919-7cf5a9dbab5b?w=400&h=400&fit=crop&sat=-100",
        "https://images.unsplash.com/photo-1557800636-894a64c1696f?w=400&h=400&fit=crop&sat=-100"
    };
    
    public MandarinGeneratorJob(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task GenerateMandarinAsync()
    {
        var now = DateTime.UtcNow;
        
        var nextMidnight = now.Date.AddDays(1);
        
        var mandarin = new Mandarin
        {
            Name = $"Мандаринка №{_random.Next(1000, 9999)}",
            ImageUrl = MandarinImages[_random.Next(MandarinImages.Length)],
            CreatedAt = now,
            SpoilAt = nextMidnight,
            Status = MandarinStatus.InAuction
        };
        
        _context.Mandarins.Add(mandarin);

        var auction = new Auction(
            mandarinId: mandarin.Id,
            startingPrice: 100,
            buyoutPrice: 1000,
            minBidIncrement: 100,
            endTime: nextMidnight
        );
        
        _context.Auctions.Add(auction);

        await _context.SaveChangesAsync();
    }
}