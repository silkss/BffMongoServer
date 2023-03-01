namespace BffLogger;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Runtime.Versioning;

[UnsupportedOSPlatform("browser")]
[ProviderAlias("BffLogger")]
public class BffLoggerProvider : ILoggerProvider
{
    private BffLoggerConfiguration _currentConfig;
    private readonly IDisposable? _onChangeToken;
    private readonly ConcurrentDictionary<string, BffLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);
    private readonly TelegramLogger _telegramLogger;

    public BffLoggerProvider(
        IOptionsMonitor<BffLoggerConfiguration> config)
    {
        _currentConfig = config.CurrentValue;
        _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
        _telegramLogger = new();
    }
    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, name => new BffLogger(name, GetCurrentConfig,_telegramLogger));
    private BffLoggerConfiguration GetCurrentConfig() => _currentConfig;
    public void Dispose()
    {
        _loggers.Clear();
        _onChangeToken?.Dispose();
    }
}
