using Microsoft.Extensions.Logging;

namespace TraderBot.Notifier;

public class Notifier
{
	private TelegramNotifier? _telegram;
	private readonly ILogger<Notifier> _logger;

	private void logToTelegram(string msg)
	{
		if (_telegram is not null)
		{
			_telegram.SendMessageAsync(msg);
		}
	}
	public Notifier(ILogger<Notifier> logger, bool withTelegram = true)
	{
		if (withTelegram)
		{
			_telegram = new();
		}
		_logger = logger;
	}
    public void LogInformation(string msg, bool toTelegram = false)
    {
        _logger.LogInformation(msg);
        if (toTelegram && _telegram is not null)
        {
            logToTelegram(msg);
        }
    }
	public void LogWarning(string msg, bool toTelegram = false)
	{
		_logger.LogWarning(msg);
        if (toTelegram && _telegram is not null)
        {
            logToTelegram(msg);
        }
    }
    public void LogError(string msg, bool toTelegram = false)
    {
        _logger.LogError(msg);
        if (toTelegram && _telegram is not null)
        {
            logToTelegram(msg);
        }
    }
    public void LogCritical(string msg, bool toTelegram = false)
	{
		_logger.LogCritical(msg);
		if (toTelegram && _telegram is not null)
        {
            logToTelegram(msg);
        }
	}
}
