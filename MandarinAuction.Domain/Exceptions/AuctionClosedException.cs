namespace MandarinAuction.Domain.Exceptions;

public class AuctionClosedException : Exception
{
    public AuctionClosedException(string message) : base(message) {}
}