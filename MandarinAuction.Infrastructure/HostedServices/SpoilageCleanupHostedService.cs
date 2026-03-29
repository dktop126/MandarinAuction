using MandarinAuction.Infrastructure.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MandarinAuction.Infrastructure.HostedServices;

/// <summary>
/// Фоновый сервис для периодической очистки испорченных мандаринов.
/// </summary>
public class SpoilageCleanupHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SpoilageCleanupHostedService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30);

    public SpoilageCleanupHostedService(
        IServiceProvider serviceProvider,
        ILogger<SpoilageCleanupHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Spoilage Cleanup Service запущен");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var job = scope.ServiceProvider.GetRequiredService<SpoilageCleanupJob>();

                var spoiledCount = await job.CleanupSpoiledMandarins();
                if (spoiledCount > 0)
                {
                    _logger.LogInformation("Количество испорченных мандаринов: {Count}, они были удалены из " +
                                           "аукционов в {Time}", spoiledCount, DateTime.UtcNow);
                }
                else
                {
                    _logger.LogInformation("Испортившихся мандаринов нет");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при очистке испорченных" +
                                     " мандаринов в {Time}", DateTime.UtcNow);
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Spoilage Cleanup Service остановлен");
    }
}