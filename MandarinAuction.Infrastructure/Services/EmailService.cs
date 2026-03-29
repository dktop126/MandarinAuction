using MandarinAuction.Application.Interfaces;
using MandarinAuction.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace MandarinAuction.Infrastructure.Services;

/// <summary>
/// Сервис для отправки email-уведомлений.
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendOtpCodeAsync(string userEmail, string? userOtpCode)
    {
        _logger.LogInformation(
            "[OTP] Email: {Email}, Код: {Code}",
            userEmail,
            userOtpCode);

        return Task.CompletedTask;
    }

    public Task SendOutbidNotificationAsync(string userEmail, Guid auctionId, decimal newPrice)
    {
        _logger.LogInformation(
            "[Уведомление] Email: {Email}, Аукцион: {AuctionId}, Новая цена: {Price}",
            userEmail,
            auctionId,
            newPrice
        );
        return Task.CompletedTask;
    }

    public Task SendWinReceiptNotificationAsync(string userEmail, Guid auctionId, decimal price)
    {
        _logger.LogInformation(
            "[Чек] Email: {Email}, Аукцион: {AuctionId}, Сумма: {Price}",
            userEmail,
            auctionId,
            price
        );
        return Task.CompletedTask;
    }
}