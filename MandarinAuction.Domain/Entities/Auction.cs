using MandarinAuction.Domain.Enums;
using MandarinAuction.Domain.Exceptions;

namespace MandarinAuction.Domain.Entities;

/// <summary>
/// Сущность, представляющая аукцион.
/// </summary>
public class Auction
{
    public Guid Id { get; private set; }
    public Guid MandarinId { get; private set; }
    public decimal StartingPrice { get; private set; }
    public decimal CurrentPrice { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public Guid? PreviousBidderId { get; private set; }
    public Guid? LastBidderId { get; private set; }
    public AuctionStatus Status { get; private set; }

    public byte[]? RowVersion { get; set; }

    public Auction(Guid mandarinId, decimal startingPrice, DateTime endTime)
    {
        Id = Guid.NewGuid();
        MandarinId = mandarinId;
        StartingPrice = startingPrice;
        CurrentPrice = startingPrice;
        StartTime = DateTime.UtcNow;
        EndTime = endTime;
        Status = AuctionStatus.Active;
    }

    /// <summary>
    /// Делает ставку. Проверяет статус аукциона и минимальную сумму.
    /// </summary>
    /// <exception cref="AuctionClosedException">Если аукцион закрыт.</exception>
    /// <exception cref="BidTooLowException">Если ставка меньше текущей или является максимальной.</exception>
    public void PlaceBid(Guid newBidderId, decimal amount)
    {
        if (Status == AuctionStatus.Closed || DateTime.UtcNow > EndTime)
            throw new AuctionClosedException("Аукцион уже завершен.");

        if (amount <= CurrentPrice)
            throw new BidTooLowException("Ставка должна быть выше текущей.");

        if (newBidderId == LastBidderId)
            throw new BidTooLowException("Ваша ставка уже является максимальной.");

        PreviousBidderId = LastBidderId;
        LastBidderId = newBidderId;
        CurrentPrice = amount;
    }

    public void CloseAsSpoiled()
    {
        if (Status is AuctionStatus.Closed or AuctionStatus.Finished)
            return;
        Status = AuctionStatus.Closed;
    }

    public void Finish()
    {
        if (Status != AuctionStatus.Active)
            return;
        Status = AuctionStatus.Finished;
    }
}