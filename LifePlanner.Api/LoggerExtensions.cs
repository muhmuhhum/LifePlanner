using System.Runtime.CompilerServices;

namespace LifePlanner.Api;

public static class LoggerExtensions
{
    public static void LogControllerError(this ILogger logger, Exception exception, [CallerMemberName] string memberName = "")
    {
        logger.LogError(exception, "Unhandled exception in {MemberName}", memberName);
    }
}