namespace MandarinAuction.Application.Interfaces;

/// <summary>
/// Интерфейс сервиса для отправки email-уведомлений.
/// </summary>
public interface IEmailService
{
    Task SendOtpCodeAsync(string userEmail, string? userOtpCode);
    Task SendOutbidNotificationAsync(string userEmail, Guid auctionId, decimal newPrice);
    Task SendWinReceiptNotificationAsync(string userEmail, Guid auctionId, decimal price);
}