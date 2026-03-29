namespace MandarinAuction.Domain.Exceptions;

/// <summary>
/// Исключение, выбрасываемое, когда запрашиваемый ресурс не найден.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}