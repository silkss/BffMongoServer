namespace BffLogger;

using Microsoft.Extensions.Logging;
using System;

public class BffLogger : ILogger
{
    private readonly TelegramLogger _telegramLogger;
    private readonly string _name;
    private readonly Func<BffLoggerConfiguration> _getCurrentConfig;
    public BffLogger(
        string name,
        Func<BffLoggerConfiguration> getCurrentConfig,
        TelegramLogger telegramLogger) =>
        (_name, _getCurrentConfig, _telegramLogger) = (name, getCurrentConfig, telegramLogger);

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull =>
        default!;
    public bool IsEnabled(LogLevel logLevel) => 
        _getCurrentConfig().LogLevelToColorMap.ContainsKey(logLevel);

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }
        var config = _getCurrentConfig();
        if (config.EventId == 0 || config.EventId == eventId.Id)
        {
            ConsoleColor originalColor = Console.ForegroundColor;

            Console.ForegroundColor = config.LogLevelToColorMap[logLevel];
            Console.WriteLine($"[{eventId.Id,2}: {logLevel,-12}]");

            Console.ForegroundColor = originalColor;
            Console.Write($"\t{_name} - ");

            var msg = formatter(state, exception);
            Console.ForegroundColor = config.LogLevelToColorMap[logLevel];
            Console.Write(msg);
            if (config.ToTelegram(logLevel))
            {
                _telegramLogger.SendMessage(msg);
            }
            Console.ForegroundColor = originalColor;
            Console.WriteLine();
        }
    }
}
