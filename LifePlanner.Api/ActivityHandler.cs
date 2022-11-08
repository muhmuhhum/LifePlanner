using LifePlanner.Api.Domain;

namespace LifePlanner.Api;

public class ActivityHandler : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<ActivityHandler> _logger;

    public ActivityHandler(IServiceProvider provider, ILogger<ActivityHandler> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _provider.CreateScope();
            var manager = scope.ServiceProvider.GetService<IActivityManager>();

            if (manager is null)
            {
                throw new Exception("Manager is not known");
            }

            var activities = await manager.GetAll();
            var now = DateTime.UtcNow;
            foreach (var activity in activities)
            {
                //handle interval
                var shouldTrigger = true;
                if (activity.LastExecution is { } lastExecution)
                {
                    shouldTrigger = CheckInterval(lastExecution, activity, now);
                }
                if (shouldTrigger is false)
                {
                    continue;
                }
                shouldTrigger = CheckNotificationTime(activity, now);


                if (shouldTrigger)
                {
                    //send notification
                }
            }
            Thread.Sleep(900_000);
        }
    }

    private bool CheckNotificationTime(Activity activity, DateTime now)
    {
        bool shouldTrigger = true;
        switch (activity.NotificationTime)
        {
            case NotificationTime.Morning:
                if (now.TimeOfDay < new TimeSpan(6, 0, 0))
                {
                    shouldTrigger = false;
                }

                break;
            case NotificationTime.Noon:
                if (now.TimeOfDay < new TimeSpan(12, 0, 0))
                {
                    shouldTrigger = false;
                }

                break;
            case NotificationTime.Afternoon:
                if (now.TimeOfDay < new TimeSpan(18, 0, 0))
                {
                    shouldTrigger = false;
                }

                break;
            case NotificationTime.Night:
                if (now.TimeOfDay < new TimeSpan(23, 0, 0))
                {
                    shouldTrigger = false;
                }

                break;
            case NotificationTime.Custom:
                if (activity.CustomNotificationTime is null)
                {
                    shouldTrigger = false;
                    _logger.LogError("Activity has NotificationTime custom without CustomNotificationTime");
                    break;
                }

                if (now.TimeOfDay < activity.CustomNotificationTime.Value.ToTimeSpan())
                    shouldTrigger = false;
                break;
        }

        return shouldTrigger;
    }

    private bool CheckInterval(DateTime lastExecution, Activity activity,  DateTime now)
    {
        bool shouldTrigger = true;
        switch (activity.Interval)
        {
            case ExecutionInterval.Daily:
                if (now.Date != lastExecution.AddDays(1))
                    shouldTrigger = false;
                break;
            case ExecutionInterval.Weekly:
                if (now.Date != lastExecution.AddDays(7))
                    shouldTrigger = false;
                break;
            case ExecutionInterval.Monthly:
                if (now.Date != lastExecution.AddMonths(1))
                    shouldTrigger = false;
                break;
            case ExecutionInterval.Semiannual:
                if (now.Date != lastExecution.AddDays(180))
                    shouldTrigger = false;
                break;
            case ExecutionInterval.Annual:
                if (now.Date != lastExecution.AddYears(1))
                    shouldTrigger = false;
                break;
            case ExecutionInterval.Custom:
                if (activity.CustomInterval is null)
                {
                    _logger.LogError("Activity has Interval custom without CustomInterval");
                    shouldTrigger = false;
                    break;
                }

                if (now.Date != lastExecution.AddDays(activity.CustomInterval.Value))
                    shouldTrigger = false;
                break;
        }

        return shouldTrigger;
    }
}