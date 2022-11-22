using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace Notifier;

public class BaseNotifier
{
	private TelegramNotifier? _telegram;
	private readonly ILogger<BaseNotifier> _logger;
    private void Log(LogLevel level, string msg, bool toTelegram = false)
    {
        try
        {
            switch (level)
            {
                case LogLevel.Criticl:
                    _logger.LogCritical(msg);
                    break;
                case LogLevel.Error:
                    _logger.LogError(msg);
                    break;
                case LogLevel.Info:
                    _logger.LogInformation(msg);
                    break;
                case LogLevel.Warning:
                    _logger.LogWarning(msg);
                    break;
                default:
                    break;
            }
            if (toTelegram)
            {
                logToTelegram(msg);
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
    }
    private void logToTelegram(string msg)
	{
		if (_telegram is not null)
		{
			_telegram.SendMessageAsync(msg);
		}
	}
	public BaseNotifier(ILogger<BaseNotifier> logger, bool withTelegram = true)
	{
#if DEBUG
        _telegram = null;
#else
        if (withTelegram)
		{
			_telegram = new();
		}
#endif
		_logger = logger;
	}
    public void LogInformation(string msg, bool toTelegram = false) =>
        Log(LogLevel.Info, msg, toTelegram);
	public void LogWarning(string msg, bool toTelegram = false) =>
        Log(LogLevel.Warning, msg, toTelegram);
    public void LogError(string msg, bool toTelegram = false) =>
        Log(LogLevel.Error, msg, toTelegram);
    public void LogCritical(string msg, bool toTelegram = false) =>
        Log(LogLevel.Criticl, msg, toTelegram);		
}

internal enum LogLevel
{
    Criticl,
    Warning,
    Info,
    Error
}
