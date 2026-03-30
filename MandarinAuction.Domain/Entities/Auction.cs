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
    public Mandarin? Mandarin { get; set; }
    public decimal StartingPrice { get; private set; }
    public decimal CurrentPrice { get; private set; }
    public decimal? BuyoutPrice { get; private set; }
    public decimal MinBidIncrement { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public Guid? PreviousBidderId { get; private set; }
    public Guid? LastBidderId { get; private set; }
    public AuctionStatus Status { get; private set; }

    public byte[]? RowVersion { get; set; }

    public Auction(Guid mandarinId, decimal startingPrice, decimal? buyoutPrice, decimal minBidIncrement, DateTime endTime)
    {
        Id = Guid.NewGuid();
        MandarinId = mandarinId;
        StartingPrice = startingPrice;
        CurrentPrice = startingPrice;
        BuyoutPrice = buyoutPrice;
        MinBidIncrement = minBidIncrement;
        StartTime = DateTime.UtcNow;
        EndTime = endTime;
        Status = AuctionStatus.Active;
    }

    /// <summary>
    /// Выкупает лот по цене выкупа. Закрывает аукцион немедленно.
    /// </summary>
    /// <param name="buyerId">ID покупателя</param>
    /// <exception cref="AuctionClosedException">Если аукцион закрыт.</exception>
    /// <exception cref="InvalidOperationException">Если нет цены выкупа.</exception>
    public void Buyout(Guid buyerId)
    {
        if (Status == AuctionStatus.Closed || DateTime.UtcNow > EndTime)
            throw new AuctionClosedException("Аукцмон уже завершен.");

        if (!BuyoutPrice.HasValue)
            throw new InvalidOperationException("Цена выкупа не установлена для этого лота.");
        
        PreviousBidderId = LastBidderId;
        LastBidderId = buyerId;
        CurrentPrice = BuyoutPrice.Value;
        Status = AuctionStatus.Finished;
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

        var minimumBid = CurrentPrice + MinBidIncrement;
        if (amount < minimumBid)
            throw new BidTooLowException($"Ставка должна быть выше текущей как минимум на {MinBidIncrement} ₽." +
                                         $" Минимальная допустимая ставка: {minimumBid} ₽.");

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