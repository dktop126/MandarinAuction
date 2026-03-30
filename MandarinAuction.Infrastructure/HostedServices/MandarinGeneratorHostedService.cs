using MandarinAuction.Infrastructure.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MandarinAuction.Infrastructure.HostedServices;

/// <summary>
/// Фоновый сервис для периодической генерации новых мандаринов.
/// </summary>
public class MandarinGeneratorHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MandarinGeneratorHostedService> _logger;
    private readonly TimeSpan _generationInterval = TimeSpan.FromMinutes(10);

    public MandarinGeneratorHostedService(
        IServiceProvider serviceProvider,
        ILogger<MandarinGeneratorHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Mandarin Generator Service запущен");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var job = scope.ServiceProvider.GetRequiredService<MandarinGeneratorJob>();

                await job.GenerateMandarinAsync();
                
                _logger.LogInformation("Новый мандарин сгенерирован в {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при генерации мандарина в {Time}", DateTime.UtcNow);
            }
            
            await  Task.Delay(_generationInterval, stoppingToken);
        }
        
        _logger.LogInformation("Mandarin Generation Service остановлен");
    }
}