﻿namespace Notifier.Implemintations.Base;

public abstract class Logger : IBffLogger
{
    protected abstract void Log(LogLevel level, string msg);
    public void LogCritical(string msg, bool toTelegram = false) => Log(LogLevel.Criticl, msg);
    public void LogError(string msg, bool toTelegram = false) => Log(LogLevel.Error, msg);
    public void LogInformation(string msg, bool toTelegram = false) => Log(LogLevel.Info, msg);
    public void LogWarning(string msg, bool toTelegram = false) => Log(LogLevel.Warning, msg);
}
