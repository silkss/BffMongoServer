using ContainerStore.Connectors;
using ContainerStore.Traders.Base;
using Microsoft.AspNetCore.Mvc;
using MongoDbSettings;
using Strategies;
using Strategies.Enums;
using Strategies.TradeUnions;
using System;
using System.Linq;
using System.Text;
using TraderBot.Notifier;

namespace ContainerStore.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class McAPIController : ControllerBase
{
	private readonly Notifier _logger;
	private readonly IConnector _connector;
	private readonly StrategyService _strategyService;
	private readonly Trader _trader;

	private readonly TimeSpan _fridayDeadLine = new TimeSpan(hours: 12, minutes: 00, seconds: 00);

	private string straddleWorkingMessage(MainStrategy strategy) =>
		$"Straddle working:\n" +
		$"OpenPnl: {strategy.GetOpenStraddle()?.GetCurrencyPnl()}\n" +
		$"TargetPnl: {strategy.StraddleSettings?.StraddleTargetPnl}\n" +
		$"--------------------------\n" +
		$"CloseDate: ~{strategy.GetApproximateCloseDate()}\n" +
		$"CreatedDate: {strategy.GetOpenStraddle()?.CreatedTime}";

	private string parseSignal(string signal, MainStrategy strategy, double price, Notifier notifier) => signal.Trim().ToLower() switch
	{
		"open" => strategy.GetOpenStraddleStatus(notifier) switch
		{
			StraddleStatus.NotExist => openStraddle(strategy, price),
			StraddleStatus.Expired => closeAndOpenStraddle(strategy, price, $"Expired:" +
				$"opening: {strategy.GetOpenStraddle()?.CreatedTime}"),
			StraddleStatus.InProfit => closeAndOpenStraddle(strategy, price, $"InProfit:\n" +
				$"target: {strategy.StraddleSettings?.StraddleTargetPnl}\n" +
				$"opening: {strategy.GetOpenStraddle()?.GetCurrencyPnl()}"),
			StraddleStatus.ClosuredProfitLevelReached => closeAndOpenStraddle(strategy, price, $"Unclosured PL:\n" +
                $"Pnl: {strategy.GetOpenStraddle()?.GetCurrencyPnl()}"),
			StraddleStatus.UnClosuredProfitLevelReached => closeAndOpenStraddle(strategy, price, $"Closured PNL:\n" +
                $"Pnl: {strategy.GetOpenStraddle()?.GetCurrencyPnl()}"),
			StraddleStatus.NotOpen => closeAndOpenStraddle(strategy, price, "Не успел открыться"),
			StraddleStatus.Working => straddleWorkingMessage(strategy),
			_ => throw new ArgumentException("Неизвестный статус контейнера!!")
		},
		"close" => strategy.GetOpenStraddleStatus(notifier) switch
		{
			StraddleStatus.Expired => closeStraddle(strategy.GetOpenStraddle(), "Expired:"),
			StraddleStatus.InProfit => closeStraddle(strategy.GetOpenStraddle(), "InProfit"),
			StraddleStatus.ClosuredProfitLevelReached => closeStraddle(strategy.GetOpenStraddle(), "Closured PL"),
			StraddleStatus.UnClosuredProfitLevelReached => closeStraddle(strategy.GetOpenStraddle(), "Unclosured PL"),
			StraddleStatus.NotOpen => closeStraddle(strategy.GetOpenStraddle(), "Не успел открыться"),
			StraddleStatus.Working => straddleWorkingMessage(strategy),
			StraddleStatus.NotExist => "There is no open straddle in the container.",
			_ => throw new ArgumentException("Неизвестный статус контейнера!!"),
		},
		"alarmclose" => strategy.GetOpenStraddleStatus(notifier) switch
		{
			StraddleStatus.NotExist => "There is no open straddle in the container.",
			_ => closeStraddle(strategy.GetOpenStraddle(), "Alarm stop."),
		},
		_ => $"unknow signal: {signal}."
	};
	private string closeStraddle(Straddle? straddle, string message)
	{
		if (straddle is null) throw new ArgumentNullException("while closing straddle it cant be null!");
		straddle.Close(_connector);
		return $"Closing straddle. Reasong: {message}";
	}

	private string openStraddle(MainStrategy mainStrategy, double price)
	{
		if (DateTime.Now.DayOfWeek == DayOfWeek.Friday && DateTime.Now.TimeOfDay >= _fridayDeadLine) 
		{
			return $"Cant open straddle. Because: {DateTime.Now.DayOfWeek} {DateTime.Now.TimeOfDay}";
		}
		var optionclass = _connector
			.GetOptionTradingClass(mainStrategy.Instrument.Id, mainStrategy.GetApproximateExpirationDate());
		if (optionclass == null)
		{
			return "Нет подходящего опционного класса.";
		}
		var baseStrike = optionclass.Strikes.MinBy(s => Math.Abs(s - price));
		var baseStrikeIdx = optionclass.Strikes.FindIndex(s => s == baseStrike);
		var closureCallStike = optionclass
			.Strikes[baseStrikeIdx + (mainStrategy.ClosureSettings?.ClosureStrikeStep ?? 0)];

		var closurePutStrike = optionclass
			.Strikes[baseStrikeIdx - (mainStrategy.ClosureSettings?.ClosureStrikeStep ?? 0)];

		_connector
			.RequestCall(mainStrategy.Instrument, baseStrike, optionclass.ExpirationDate, out var baseCall)
			.RequestCall(mainStrategy.Instrument, closureCallStike, optionclass.ExpirationDate, out var closureCall)
			.RequestPut(mainStrategy.Instrument, baseStrike, optionclass.ExpirationDate, out var basePut)
			.RequestPut(mainStrategy.Instrument, closurePutStrike, optionclass.ExpirationDate, out var closurePut);

		if (baseCall is null)
		{
			return $"не удалось запросить Call для ноги стредла:\n" +
				$"parentId: {mainStrategy.Instrument.Id}. Strike:{baseStrike}. ExpDate: {optionclass.ExpirationDate}.";

		}
		if (closureCall is null)
		{
			return $"не удалось запросить Call для замыкания:\n" +
				$"parentId: {mainStrategy.Instrument.Id}. Strike:{baseStrike}. ExpDate: {optionclass.ExpirationDate}.";
		}
		if (basePut is null)
		{
			return $"не удалось запросить Put для ноги стредла:\n" +
				$"parentId: {mainStrategy.Instrument.Id}. Strike:{baseStrike}. ExpDate: {optionclass.ExpirationDate}.";
		}
		if (closurePut is null)
		{
			return $"не удалось запросить Put для замыкания:\n" +
				$"parentId: {mainStrategy.Instrument.Id}. Strike:{baseStrike}. ExpDate: {optionclass.ExpirationDate}.";
		}

		_connector
			.RequestMarketData(baseCall)
			.RequestMarketData(basePut)
			.RequestMarketData(closureCall)
			.RequestMarketData(closurePut);

        mainStrategy.AddStraddle(new Straddle(baseCall, closureCall, basePut, closurePut), _connector);
		return "Straddle добавлен.";
	}

	private string closeAndOpenStraddle(MainStrategy mainStrategy, double price, string message) =>
		new StringBuilder()
			.AppendLine(closeStraddle(mainStrategy.GetOpenStraddle(), message))
			.AppendLine(openStraddle(mainStrategy, price))
			.ToString();

	public McAPIController(Notifier logger, IConnector connector, StrategyService strategyService , Trader trader)
	{
		_logger = logger;
		_connector = connector;
        _strategyService = strategyService;
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
