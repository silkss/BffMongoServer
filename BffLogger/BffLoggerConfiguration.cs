namespace BffLogger;

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

public class BffLoggerConfiguration
{
    public int EventId { get; set; }
    public Dictionary<LogLevel, ConsoleColor> LogLevelToColorMap { get; set; } = new()
    {
        [LogLevel.Information] = ConsoleColor.Green,
        [LogLevel.Warning] = ConsoleColor.DarkYellow,
        [LogLevel.Error] = ConsoleColor.Red,
        [LogLevel.Critical] =ConsoleColor.DarkRed,
    };

    public bool ToTelegram(LogLevel level) => level switch
    {
        LogLevel.Information => true,
        LogLevel.Critical => true,
        _ => false
    };
}
