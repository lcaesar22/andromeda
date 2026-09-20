using Andromeda.Features.GetAllSatellites;

namespace Andromeda.Infrastructure;

public class CacheWarmingService(IServiceScopeFactory scopeFactory, ILogger<CacheWarmingService> logger) :
    BackgroundService
{
    private static readonly TimeSpan WarmInterval = TimeSpan.FromMinutes(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting Warming");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await WarmCacheAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Cache warming cycle failed");
            }
            
            await Task.Delay(WarmInterval, stoppingToken);
        }
        logger.LogInformation("Stopping Warming");
    }

    private async Task WarmCacheAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ISatelliteRepository>();

        var defaultFilter = new SatelliteFilter(null, null, null, null);
        await repo.GetAllAsync(defaultFilter, page: 1, pageSize: 20 );
        
        logger.LogInformation("Cache warmed for default satellite listing");
    }
}