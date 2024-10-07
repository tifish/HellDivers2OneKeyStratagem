using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using ZLogger;
using ZLogger.Providers;

public static class LogFactory
{
    private static readonly ILoggerFactory _factory = LoggerFactory.Create(logging =>
    {
        logging.ClearProviders();

        if (Design.IsDesignMode)
        {
            logging.SetMinimumLevel(LogLevel.Error);
            return;
        }

        logging.SetMinimumLevel(LogLevel.Trace);

        logging.AddZLoggerRollingFile(options =>
        {
            // File name determined by parameters to be rotated
            options.FilePathSelector = (timestamp, sequenceNumber)
                => $"Logs/{timestamp.ToLocalTime():yyyy-MM-dd}_{sequenceNumber:000}.log";
            // The period of time for which you want to rotate files at time intervals.
            options.RollingInterval = RollingInterval.Day;
        });
    });

    public static ILogger CreateLogger<T>()
    {
        return _factory.CreateLogger<T>();
    }
}
