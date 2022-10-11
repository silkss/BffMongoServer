using ContainerStore.Common.Enums;
using ContainerStore.Connectors;
using ContainerStore.Data.Models;
using ContainerStore.Traders.Base;
using ContainerStore.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
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
	private readonly ContainersService _containersService;
	private readonly Trader _trader;

	private string straddleWorkingMessage(Container container) =>
		$"Straddle working:\n" +
        $"OpenPnl: {container.CurrencyOpenPnl}\n" +
        $"TargetPnl: {container.StraddleTargetPnl}\n" +
		$"--------------------------\n" +
        $"CloseDate: ~{container.ApproximateCloseDate}\n" +
        $"CreatedDate: {container.OpenStraddle?.CreatedTime}";
	private StraddleStatus statusOfOpenStraddle(Container container)
	{
		if (container.OpenStraddle is null)
			return StraddleStatus.NotExist;

		if (container.CurrencyOpenPnl >= container.StraddleTargetPnl)
			return StraddleStatus.InProfit;

		if (container.OpenStraddle.CreatedTime > container.ApproximateCloseDate)
			return StraddleStatus.Expired;

		if (container.OpenStraddle.IsDone() is false)
			return StraddleStatus.NotOpen;

		return StraddleStatus.Working;
	}

	private string parseSignal(string signal, Container container, double price) => signal.Trim().ToLower() switch
	{
		"open" => statusOfOpenStraddle(container) switch
		{
			StraddleStatus.NotExist => openStraddle(container, price),
			StraddleStatus.Expired => closeAndOpenStraddle(container,price, $"Expired:" +
                $"opened: {container.OpenStraddle?.CreatedTime}"),
			StraddleStatus.InProfit => closeAndOpenStraddle(container, price, $"InProfit:\n" +
                $"target: {container.StraddleTargetPnl}\n" +
                $"open: {container.CurrencyOpenPnl}"),
            StraddleStatus.NotOpen => closeAndOpenStraddle(container, price, "Не успел открыться"),
			StraddleStatus.Working => straddleWorkingMessage(container),
			_ => throw new ArgumentException("Неизвестный статус контейнера!!")
        },
		"close" => statusOfOpenStraddle(container) switch
		{
			StraddleStatus.Expired => closeStraddle(container.OpenStraddle, "Expired:"),
			StraddleStatus.InProfit => closeStraddle(container.OpenStraddle, "InProfit"), 
			StraddleStatus.NotOpen => closeStraddle(container.OpenStraddle, "Не успел открыться"),
			StraddleStatus.Working => straddleWorkingMessage(container),
            StraddleStatus.NotExist => "Не существует открытого стрэддла!",
			_ => throw new ArgumentException("Неизвестный статус контейнера!!"),
		},
		_ => throw new ArgumentException("Неизвестный сигнал.")
	};
	private string closeStraddle(Straddle? straddle, string message)
	{
		if (straddle is null) throw new ArgumentNullException("while closing straddle it cant be null!");
		straddle.Close();
		return message;
	}
    private string openStraddle(Container container, double price)
    {
        var optionclass = _connector
            .GetOptionTradingClass(container.ParentInstrument.Id, container.GetApproximateExpirationDate());
        if (optionclass == null)
        {
            return "Нет подходящего опционного класса.";
        }
        var basestrike = optionclass.Strikes.MinBy(s => Math.Abs(s - price));
        var basestrikeIdx = optionclass.Strikes.FindIndex(s => s == basestrike);
        var closureCallStike = optionclass.Strikes[basestrikeIdx + container.ClosureStrikeStep];
        var closurePutStrike = optionclass.Strikes[basestrikeIdx - container.ClosureStrikeStep];

		_connector
			.RequestCall(container.ParentInstrument, basestrike, optionclass.ExpirationDate, out var baseCall)
			.RequestCall(container.ParentInstrument, closureCallStike, optionclass.ExpirationDate, out var closureCall)
			.RequestPut(container.ParentInstrument, basestrike, optionclass.ExpirationDate, out var basePut)
			.RequestPut(container.ParentInstrument, closurePutStrike, optionclass.ExpirationDate, out var closurePut);

        if (baseCall is null)
        {
            return $"не удалось запросить Call для ноги стредла:\n" +
                $"parentId: {container.ParentInstrument.Id}. Strike:{basestrike}. ExpDate: {optionclass.ExpirationDate}.";

        }
        if (closureCall is null)
        {
            return $"не удалось запросить Call для замыкания:\n" +
                $"parentId: {container.ParentInstrument.Id}. Strike:{basestrike}. ExpDate: {optionclass.ExpirationDate}.";
        }
        if (basePut is null)
        {
            return $"не удалось запросить Put для ноги стредла:\n" +
                $"parentId: {container.ParentInstrument.Id}. Strike:{basestrike}. ExpDate: {optionclass.ExpirationDate}.";
        }
        if (closurePut is null)
        {
            return $"не удалось запросить Put для замыкания:\n" +
                $"parentId: {container.ParentInstrument.Id}. Strike:{basestrike}. ExpDate: {optionclass.ExpirationDate}.";
        }

        _connector
            .RequestMarketData(baseCall)
            .RequestMarketData(basePut)
            .RequestMarketData(closureCall)
            .RequestMarketData(closurePut);

        container.AddStraddle(new Straddle(baseCall, closureCall, basePut, closurePut));
        return "Straddle добавлен.";
    }
	private string closeAndOpenStraddle(Container container, double price, string message) =>
		new StringBuilder()
			.AppendLine(closeStraddle(container.OpenStraddle, message))
			.AppendLine(openStraddle(container, price))
			.ToString();

    public McAPIController(Notifier logger, IConnector connector, ContainersService containersService, Trader trader)
	{
		_logger = logger;
		_connector = connector;
		_containersService = containersService;
		_trader = trader;
	}

	[HttpGet]
	public IActionResult Get(string symbol, double price, string account, string type)
	{
		var sb = new StringBuilder();
		sb.AppendLine($"SIGNAL:\n" +
            $"Symbol:{symbol}\n" +
            $"Price:{price}\n" +
            $"Account:{account}\n" +
            $"Type:{type}");
		var container = _trader.GetContainer(symbol, account);
		if (container is null)
		{
			sb.AppendLine("No container in trade.");
            _logger.LogInformation(sb.ToString(), toTelegram: true);
            return Ok();
		}
		sb.AppendLine(parseSignal(type, container, price));
		_logger.LogInformation(sb.ToString(), toTelegram: true);
		return Ok();
	}
}
