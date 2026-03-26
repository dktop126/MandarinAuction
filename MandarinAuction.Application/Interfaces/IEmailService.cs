namespace MandarinAuction.Application.Interfaces;

public interface IEmailService
{
    Task SendOtpCodeAsync(string userEmail, string? userOtpCode);
    Task SendOutbidNotificationAsync(string userEmail, Guid auctionId, decimal newPrice);
    Task SendWinReceiptNotificationAsync(string userEmail, Guid auctionId, decimal price);
}