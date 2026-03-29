namespace MandarinAuction.Domain.Exceptions;

/// <summary>
/// Исключение, выбрасываемое при попытке сделать ставку ниже текущей или повторную ставку от того же пользователя.
/// </summary>
public class BidTooLowException : Exception
{
    public BidTooLowException(string message) : base(message) {}
}