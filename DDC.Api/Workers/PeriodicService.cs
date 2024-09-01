namespace DDC.Api.Workers;

abstract class PeriodicService : BackgroundService
{
    readonly TimeSpan _period;

    protected PeriodicService(TimeSpan period, ILogger<PeriodicService> logger)
    {
        _period = period;
        Logger = logger;
    }

    protected ILogger<PeriodicService> Logger { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await OnStartAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await OnTickAsync(stoppingToken);
            }
            catch (Exception exn)
            {
                Logger.LogError(exn, "Failed to execute periodic task.");
            }

            await Task.Delay(_period, stoppingToken);
        }

        await OnStopAsync(stoppingToken);
    }

    protected virtual Task OnStartAsync(CancellationToken stoppingToken) => Task.CompletedTask;
    protected virtual Task OnTickAsync(CancellationToken stoppingToken) => Task.CompletedTask;
    protected virtual Task OnStopAsync(CancellationToken stoppingToken) => Task.CompletedTask;
}
