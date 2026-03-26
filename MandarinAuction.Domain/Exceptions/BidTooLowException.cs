namespace MandarinAuction.Domain.Exceptions;

public class BidTooLowException : Exception
{
    public BidTooLowException(string message) : base(message) {}
}