namespace MandarinAuction.Domain.Exceptions;

/// <summary>
/// Исключение, выбрасываемое при попытке сделать ставку в закрытый аукцион.
/// </summary>
public class AuctionClosedException : Exception
{
    public AuctionClosedException(string message) : base(message) {}
}