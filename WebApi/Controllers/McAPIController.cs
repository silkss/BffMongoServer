using Notifier;
using Connectors;
using Microsoft.AspNetCore.Mvc;
using Strategies.Enums;
using System;
using System.Text;
using Traders.Base;
using Strategies.Strategies;
using Strategies.Builders;

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
			StraddleStatus.NotExist => LongStraddleStrategyBuilder.OpenStraddle(_connector,strategy, price),
			StraddleStatus.Expired => LongStraddleStrategyBuilder.CloseAndOpenStraddle(_connector, strategy, price, $"Expired:" +
				$"opening: {strategy.GetOpenStraddle()?.CreatedTime}"),
			StraddleStatus.InProfit => LongStraddleStrategyBuilder.CloseAndOpenStraddle(_connector, strategy, price, $"InProfit:\n" +
				$"target: {strategy.StraddleSettings?.StraddleTargetPnl}\n" +
				$"opening: {strategy.GetOpenStraddle()?.GetCurrencyPnl()}"),
			StraddleStatus.ClosuredProfitLevelReached => LongStraddleStrategyBuilder
                .CloseAndOpenStraddle(_connector, strategy, price, $"Unclosured PL:\n" +
					$"Pnl: {strategy.GetOpenStraddle()?.GetCurrencyPnl()}"),
			StraddleStatus.UnClosuredProfitLevelReached => LongStraddleStrategyBuilder
                .CloseAndOpenStraddle(_connector, strategy, price, $"Closured PNL:\n" +
					$"Pnl: {strategy.GetOpenPnlCurrency()}"),
			StraddleStatus.NotOpen => LongStraddleStrategyBuilder
                .CloseAndOpenStraddle(_connector, strategy, price, "Не успел открыться"),
			StraddleStatus.Working => straddleWorkingMessage(strategy),
			_ => throw new ArgumentException("Неизвестный статус контейнера!!")
		},
		"close" => strategy.GetOpenStraddleStatus(notifier) switch
		{
			StraddleStatus.Expired => LongStraddleStrategyBuilder.CloseStraddle(_connector, strategy.GetOpenStraddle(), "Expired:"),
			StraddleStatus.InProfit => LongStraddleStrategyBuilder
                .CloseStraddle(_connector, strategy.GetOpenStraddle(), "InProfit"),
			StraddleStatus.ClosuredProfitLevelReached => LongStraddleStrategyBuilder
                .CloseStraddle(_connector, strategy.GetOpenStraddle(), "Closured PL"),
			StraddleStatus.UnClosuredProfitLevelReached => LongStraddleStrategyBuilder
				.CloseStraddle(_connector, strategy.GetOpenStraddle(), "Unclosured PL"),
			StraddleStatus.NotOpen => LongStraddleStrategyBuilder
                .CloseStraddle(_connector, strategy.GetOpenStraddle(), "Не успел открыться"),
			StraddleStatus.Working => straddleWorkingMessage(strategy),
			StraddleStatus.NotExist => "There is no open straddle in the container.",
			_ => throw new ArgumentException("Неизвестный статус контейнера!!"),
		},
		"alarmclose" => strategy.GetOpenStraddleStatus(notifier) switch
		{
			StraddleStatus.NotExist => "There is no open straddle in the container.",
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
