using Connectors;
using Notifier;
using Strategies.Builders;
using Strategies.Enums;
using Strategies.Strategies;
using System;

namespace WebApi.SignalParsers;

internal static class LongStraddleSignalParser
{
    private static string closeAndOpen(int direction, MainStrategy strategy, 
        double price, string msg, IConnector connector) => direction switch
    {
        -1 => LongStraddleStrategyBuilder.CloseAndOpenStraddle(connector, strategy, price, msg),
        0 => LongStraddleStrategyBuilder.CloseStraddle(connector, strategy.GetOpenStraddle(), msg),
        1 => LongStraddleStrategyBuilder.CloseAndOpenStraddle(connector, strategy, price, msg),
        _ => throw new ArgumentException($"Неизвестное направление сигнала! {direction}")
    };

    private static string straddleWorkingMessage(MainStrategy strategy) =>
    $"Straddle working:\n" +
    $"OpenPnl: {strategy.GetOpenStraddle()?.GetCurrencyPnl()}\n" +
    $"TargetPnl: {strategy.StraddleSettings?.StraddleTargetPnl}\n" +
    $"--------------------------\n" +
    $"CloseDate: ~{strategy.GetApproximateCloseDate()}\n" +
    $"CreatedDate: {strategy.GetOpenStraddle()?.CreatedTime}";

    public static string ParseSignal(int direction, double price, 
        MainStrategy strategy, IConnector connector, 
        IBffLogger logger) => strategy.GetOpenStraddleStatus(logger) switch
    {
        StraddleStatus.NotExist => direction switch
        {
            -1 => LongStraddleStrategyBuilder.OpenStraddle(connector, strategy, price),
            0 => "No straddles for close",
            1 => LongStraddleStrategyBuilder.OpenStraddle(connector, strategy, price),
            _ => throw new ArgumentException($"Неизвестное направление сигнала! {direction}")
        },
        StraddleStatus.Expired => closeAndOpen(direction, strategy, price, "Straddle Expired", connector),
        StraddleStatus.InProfit => closeAndOpen(
            direction, strategy, price,
            $"In Profit {strategy.GetOpenPnlCurrency()} - {strategy.GetCurrentTargetPnl()}",
            connector),
        StraddleStatus.ClosuredProfitLevelReached => closeAndOpen(
            direction, strategy, price,
            $"Closured PL {strategy.GetOpenPnlCurrency()} - {strategy.GetCurrentTargetPnl()}",
            connector),
        StraddleStatus.UnClosuredProfitLevelReached => closeAndOpen(
            direction, strategy, price,
            $"UnClosured PL {strategy.GetOpenPnlCurrency()} - {strategy.GetCurrentTargetPnl()}",
            connector),
        StraddleStatus.NotOpen => closeAndOpen(
            direction, strategy, price, "Not opened!", connector),
        StraddleStatus.Working => straddleWorkingMessage(strategy),
        _ => throw new ArgumentException("Неизвестный статус стрэддла!")
    };
}
