using Microsoft.Extensions.Logging;

namespace Notifier;

public interface IBffLogger
{
    void LogInformation(string msg, bool toTelegram = false);
    void LogWarning(string msg, bool toTelegram = false);
    void LogError(string msg, bool toTelegram = false); 
    void LogCritical(string msg, bool toTelegram = false);
}
