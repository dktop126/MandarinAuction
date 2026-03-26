namespace MandarinAuction.Domain.Entities;

/// <summary>
/// Сущность, представляющая ставку.
/// </summary>
public class Bid
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AuctionId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Timestamp { get; set; } =  DateTime.UtcNow;
    public Auction? Auction { get; set; }
    public User? User { get; set; }

    public Bid(Guid auctionId, Guid userId, decimal amount)
    {
        if(amount <= 0)
            throw new ArgumentException("Сумма ставки должна быть больше нуля.", nameof(amount));
        AuctionId = auctionId;
        UserId = userId;
        Amount = amount;
        Timestamp = DateTime.UtcNow;
    }
    
    private Bid() {}
}