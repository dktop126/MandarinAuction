using MandarinAuction.Infrastructure.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MandarinAuction.Infrastructure.HostedServices;

public class SpoilageCleanupHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SpoilageCleanupHostedService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

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

                await job.CleanupSpoiledMandarins();
                
                _logger.LogInformation("Очистка испорченных мандаринов выполнена в {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при очистке испорченных мандаринов в {Time}", DateTime.UtcNow);
            }
            
            await Task.Delay(_checkInterval, stoppingToken);
        }
        
        _logger.LogInformation("Spoilage Cleanup Service остановлен");
    }
}