namespace LifePlanner.Api.Domain;

public class Activity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ExecutionInterval Interval { get; set; }
    public TimeSpan? CustomInterval { get; set; }
    public NotificationTime NotificationTime { get; set; }
    public TimeOnly? CustomNotificationTime { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? LastExecution { get; set; }
    public bool IsActive { get; set; }

    public List<User> Users { get; set; }

    public Activity(Guid id, string name, ExecutionInterval interval, TimeSpan? customInterval, NotificationTime notificationTime, TimeOnly? customNotificationTime, DateTime startDate, DateTime? lastExecution, bool isActive)
    {
        Id = id;
        Name = name;
        Interval = interval;
        CustomInterval = customInterval;
        NotificationTime = notificationTime;
        CustomNotificationTime = customNotificationTime;
        StartDate = startDate;
        LastExecution = lastExecution;
        IsActive = isActive;
    }

    public Activity()
    {
        
    }

   
}

public enum NotificationTime
{
    Morning = 0,
    Noon = 1,
    Afternoon = 2,
    Night = 3,
    Custom = 4
}

public enum ExecutionInterval
{
    Daily = 0,
    Weekly = 1,
    Monthly = 2,
    Semiannual = 3,
    Annual = 4,
    Custom = 5
}