namespace BffLogger;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using System;

public static class BffLoggerExtensions
{
    public static ILoggingBuilder AddBffLogger(
        this ILoggingBuilder builder)
    {
        builder.AddConfiguration();
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ILoggerProvider, BffLoggerProvider>());
        return builder;
    }

    public static ILoggingBuilder AddBffLogger(
        this ILoggingBuilder builder,
        Action<BffLoggerConfiguration> configure)
    {
        builder.AddBffLogger();
        builder.Services.Configure(configure);

        return builder;
    }
}
