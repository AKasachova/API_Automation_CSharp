using NLog;

public static class LoggerExtensions
{
    private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

    public static void LogInfo(this object obj, string message)
    {
        logger.Info(message);
    }

    public static void LogError(this object obj, string message)
    {
        logger.Error(message);
    }
}