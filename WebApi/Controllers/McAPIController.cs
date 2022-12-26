using Notifier;
using Connectors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using Traders.Base;
using Strategies.Strategies;
using Strategies.Builders;
using Strategies.Types;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class McAPIController : ControllerBase
{
    private readonly IConnector _connector;
    private readonly IBffLogger _logger;
	private readonly Trader _trader;

	private string straddleWorkingMessage(MainStrategy strategy) =>
		$"Straddle working:\n" +
		$"OpenPnl: {strategy.GetOpenStraddle()?.GetCurrencyPnl()}\n" +
		$"TargetPnl: {strategy.StraddleSettings?.StraddleTargetPnl}\n" +
		$"--------------------------\n" +
		$"CloseDate: ~{strategy.GetApproximateCloseDate()}\n" +
		$"CreatedDate: {strategy.GetOpenStraddle()?.CreatedTime}";

	private string parseSignal(string signal, MainStrategy strategy, double price, IBffLogger notifier) => signal.Trim().ToLower() switch
	{
		"open" => strategy.GetOpenStraddleStatus(notifier) switch
		{
            OptionStrategyStatus.NotExist => LongStraddleStrategyBuilder.OpenStraddle(_connector,strategy, price),
            OptionStrategyStatus.Expired => LongStraddleStrategyBuilder.CloseAndOpenStraddle(_connector, strategy, price, $"Expired:" +
				$"opening: {strategy.GetOpenStraddle()?.CreatedTime}"),
            OptionStrategyStatus.InProfit => LongStraddleStrategyBuilder.CloseAndOpenStraddle(_connector, strategy, price, $"InProfit:\n" +
				$"target: {strategy.StraddleSettings?.StraddleTargetPnl}\n" +
				$"opening: {strategy.GetOpenStraddle()?.GetCurrencyPnl()}"),
            OptionStrategyStatus.ClosuredProfitLevelReached => LongStraddleStrategyBuilder
                .CloseAndOpenStraddle(_connector, strategy, price, $"Unclosured PL:\n" +
					$"Pnl: {strategy.GetOpenStraddle()?.GetCurrencyPnl()}"),
            OptionStrategyStatus.UnClosuredProfitLevelReached => LongStraddleStrategyBuilder
                .CloseAndOpenStraddle(_connector, strategy, price, $"Closured PNL:\n" +
					$"Pnl: {strategy.GetOpenPnlCurrency()}"),
            OptionStrategyStatus.NotOpen => LongStraddleStrategyBuilder
                .CloseAndOpenStraddle(_connector, strategy, price, "Не успел открыться"),
            OptionStrategyStatus.Working => straddleWorkingMessage(strategy),
			_ => throw new ArgumentException("Неизвестный статус контейнера!!")
		},
		"close" => strategy.GetOpenStraddleStatus(notifier) switch
		{
            OptionStrategyStatus.Expired => LongStraddleStrategyBuilder.CloseStraddle(_connector, strategy.GetOpenStraddle(), "Expired:"),
            OptionStrategyStatus.InProfit => LongStraddleStrategyBuilder
                .CloseStraddle(_connector, strategy.GetOpenStraddle(), "InProfit"),
            OptionStrategyStatus.ClosuredProfitLevelReached => LongStraddleStrategyBuilder
                .CloseStraddle(_connector, strategy.GetOpenStraddle(), "Closured PL"),
            OptionStrategyStatus.UnClosuredProfitLevelReached => LongStraddleStrategyBuilder
				.CloseStraddle(_connector, strategy.GetOpenStraddle(), "Unclosured PL"),
            OptionStrategyStatus.NotOpen => LongStraddleStrategyBuilder
                .CloseStraddle(_connector, strategy.GetOpenStraddle(), "Не успел открыться"),
            OptionStrategyStatus.Working => straddleWorkingMessage(strategy),
            OptionStrategyStatus.NotExist => "There is no open straddle in the container.",
			_ => throw new ArgumentException("Неизвестный статус контейнера!!"),
		},
		"alarmclose" => strategy.GetOpenStraddleStatus(notifier) switch
		{
            OptionStrategyStatus.NotExist => "There is no open straddle in the container.",
			_ => LongStraddleStrategyBuilder.CloseStraddle(_connector, strategy.GetOpenStraddle(), "Alarm stop."),
		},
		_ => $"unknow signal: {signal}."
	};

	public McAPIController(IBffLogger logger, IConnector connector, Trader trader)
	{
		_logger = logger;
		_connector = connector;
		_trader = trader;
	}

	[HttpGet]
	public IActionResult Get(string symbol, double price, string account, string type)
	{
		var sb = new StringBuilder();
		sb.AppendLine($"SIGNAL: {symbol}-{type}");

		var strategy = _trader.GetStrategy(symbol, account);
		if (strategy is null)
		{
			sb.AppendLine("No container in trade.");
			sb.AppendLine($"Account: {account}. Price: {price}.");
			_logger.LogInformation(sb.ToString(), toTelegram: true);
			return Ok();
		}
		sb.AppendLine(parseSignal(type, strategy, price, _logger));
		sb.AppendLine($"Account: {account}. Price: {price}.");
#if DEBUG
		_logger.LogInformation(sb.ToString(), toTelegram: false);
#else
		_logger.LogInformation(sb.ToString(), toTelegram: true);
#endif
		return Ok();
	}
}
