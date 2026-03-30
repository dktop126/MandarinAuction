namespace MandarinAuction.Domain.Entities;

/// <summary>
/// Сущность, представляющая ставку.
/// </summary>
public class Bid
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid AuctionId { get; private set; }
    public Guid UserId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime Timestamp { get; set; }
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
}