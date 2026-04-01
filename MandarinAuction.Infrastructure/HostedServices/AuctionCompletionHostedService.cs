using MandarinAuction.Infrastructure.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MandarinAuction.Infrastructure.HostedServices;

/// <summary>
/// Фоновый сервис для ежедневной обработки аукционов в 00:00 UTC.
/// Обрабатывает испорченные мандарины и завершает аукционы с победителями.
/// </summary>
public class AuctionCompletionHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuctionCompletionHostedService> _logger;

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
                var now = DateTime.UtcNow;
                var nextMidnight = now.Date.AddDays(1); // Следующая полночь 00:00 UTC
                var delay = nextMidnight - now;

                _logger.LogInformation("Следующая обработка аукционов в {NextRun} (через {Delay})", 
                    nextMidnight, delay);

                await Task.Delay(delay, stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                var job = scope.ServiceProvider.GetRequiredService<AuctionCompletionJob>();

                var (spoiledCount, finishedCount) = await job.ProcessDailyAuctionCleanup();
                
                _logger.LogInformation(
                    "Ежедневная обработка завершена в {Time}: испорчено мандаринов: {Spoiled}, завершено аукционов: {Finished}",
                    DateTime.UtcNow, spoiledCount, finishedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке аукционов в {Time}", DateTime.UtcNow);
                // В случае ошибки ждем 1 минуту перед повторной попыткой
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
        _logger.LogInformation("Auction Completion Service остановлен");
    }
}