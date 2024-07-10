#nullable enable
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Rsb.Configurations;
using Rsb.Services;

namespace Rsb.SchedulingUtils;

public abstract class SchedulerJob<TSchedulerSettings> : BackgroundService
    where TSchedulerSettings : class, IConfigureSchedulerJob,
    IConfigureAzureServiceBus, new()
{
    // ReSharper disable once NotAccessedField.Local
    private Timer? _timer;
    protected readonly IOptions<TSchedulerSettings> _options;
    protected readonly IMessagingContext _messagingContext;
    private readonly IMessageEmitter _emitter;

    protected SchedulerJob(IOptions<TSchedulerSettings> options,
        IMessagingContext messagingContext, IMemoryCache cache)
    {
        _options = options;
        _messagingContext = messagingContext;
        _emitter = new MessageEmitter(
            new AzureServiceBusService<TSchedulerSettings>(_options, cache));
    }

    protected abstract Task PerformJob();

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var objectState = new object();

        //TODO - allow to set when the timer should start
        var startingTime = TimeSpan.Zero;
        _timer = new Timer(InnerPerform, objectState, startingTime,
            TimeSpan.FromSeconds(_options.Value.IntervalInSeconds));

        return Task.CompletedTask;
    }

    private async void InnerPerform(object? state)
    {
        await PerformJob().ConfigureAwait(false);

        await _emitter.FlushAll((ICollectMessage)_messagingContext)
            .ConfigureAwait(false);
    }
}