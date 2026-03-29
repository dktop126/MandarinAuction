using MandarinAuction.Infrastructure.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MandarinAuction.Infrastructure.HostedServices;

/// <summary>
/// Фоновый сервис для периодической обработки завершенных аукционов.
/// </summary>
public class AuctionCompletionHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuctionCompletionHostedService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30);

    public AuctionCompletionHostedService(
        IServiceProvider serviceProvider,
        ILogger<AuctionCompletionHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Auction Completion Hosted Service запущен");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var job = scope.ServiceProvider.GetRequiredService<AuctionCompletionJob>();

                var finishedCount = await job.ProcessFinishedAuctions();
                if (finishedCount > 0)
                {
                    _logger.LogInformation("Обработано завершенных аукционов: {Count}," +
                                           " уведомления отправлены в {Time}", finishedCount, DateTime.UtcNow);
                }
                else
                {
                    _logger.LogInformation("Нет завершенных аукционов для обработки");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработе завершенных аукционов в {Time}", DateTime.UtcNow);
            }
            await Task.Delay(_checkInterval, stoppingToken);
        }
        _logger.LogInformation("Auction Completion Service остановлен");
    }
}