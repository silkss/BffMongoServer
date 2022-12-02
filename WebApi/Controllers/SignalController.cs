using Connectors;
using ContainerStore.Traders.Helpers;
using Instruments.PriceRules;
using Microsoft.AspNetCore.Mvc;
using Notifier;
using Strategies;
using Strategies.Enums;
using Strategies.TradeUnions;
using System;
using System.Text;
using Traders.Base;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SignalController : ControllerBase
{
    private readonly IConnector _connector;
    private readonly IBffLogger _logger;
    private readonly Trader _trader;

    private string closeAndOpen(int direction, MainStrategy strategy, double price, string message) => direction switch
    {
        -1 => StraddleBuyerHelper.CloseAndOpenStraddle(_connector, strategy, price, "Straddle expired"),
        0 => StraddleBuyerHelper.CloseStraddle(_connector, strategy.GetOpenStraddle(), "Straddle expired"),
        1 => StraddleBuyerHelper.CloseAndOpenStraddle(_connector, strategy, price, "Straddle expired"),
        _ => throw new ArgumentException($"Неизвестное направление сигнала! {direction}")
    };

    private string straddleWorkingMessage(MainStrategy strategy) =>
        $"Straddle working:\n" +
        $"OpenPnl: {strategy.GetOpenStraddle()?.GetCurrencyPnl()}\n" +
        $"TargetPnl: {strategy.StraddleSettings?.StraddleTargetPnl}\n" +
        $"--------------------------\n" +
        $"CloseDate: ~{strategy.GetApproximateCloseDate()}\n" +
        $"CreatedDate: {strategy.GetOpenStraddle()?.CreatedTime}";

    private string parseSignal(int direction, double price, MainStrategy strategy)
        => strategy.GetOpenStraddleStatus(_logger) switch
        {
            StraddleStatus.NotExist => direction switch
            {
                -1 => StraddleBuyerHelper.OpenStraddle(_connector, strategy, price),
                0 => "No straddles for close",
                1 => StraddleBuyerHelper.OpenStraddle(_connector, strategy, price),
                _ => throw new ArgumentException($"Неизвестное направление сигнала! {direction}")
            },
            StraddleStatus.Expired => closeAndOpen(direction, strategy, price, "Straddle Expired"),
            StraddleStatus.InProfit => closeAndOpen(
                direction, 
                strategy, 
                price, 
                $"In Profit {strategy.GetOpenPnlCurrency()} - {strategy.GetCurrentTargetPnl()}"),
            StraddleStatus.ClosuredProfitLevelReached => closeAndOpen(
                direction,
                strategy,
                price,
                $"Closured PL {strategy.GetOpenPnlCurrency()} - {strategy.GetCurrentTargetPnl()}"),
            StraddleStatus.UnClosuredProfitLevelReached => closeAndOpen(
                direction,
                strategy,
                price,
                $"UnClosured PL {strategy.GetOpenPnlCurrency()} - {strategy.GetCurrentTargetPnl()}"),
            StraddleStatus.NotOpen => closeAndOpen(
                direction, strategy, price, "Not opened!"),
            StraddleStatus.Working => straddleWorkingMessage(strategy),
            _ => throw new ArgumentException("Неизвестный статус стрэддла!")
        };


    public SignalController(IConnector connector, IBffLogger logger, Trader trader)
    {
        _connector = connector;
        _logger = logger;
        _trader = trader;
    }

    [HttpGet]
    public IActionResult Get(string symbol, int direction, double price, string account)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"SIGNAL: {symbol}-{account}-{direction}");

        var strategy = _trader.GetStrategy(symbol, account);
        if (strategy is null)
        {
            sb.AppendLine("No container in trade.");
            _logger.LogInformation(sb.ToString(), toTelegram: true);
            return Ok();
        }

        sb.AppendLine(parseSignal(direction, price, strategy ));
        sb.AppendLine($"Account: {account}. Price: {price}.");
        return Ok();
    }
}
