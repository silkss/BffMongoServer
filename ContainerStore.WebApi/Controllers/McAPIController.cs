using ContainerStore.Connectors;
using ContainerStore.Data.Models;
using ContainerStore.Data.Models.TradeUnits;
using ContainerStore.Traders.Base;
using ContainerStore.WebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;
using System;
using System.Linq;

namespace ContainerStore.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class McAPIController : ControllerBase
{
	private readonly ILogger<McAPIController> _logger;
	private readonly IConnector _connector;
	private readonly ContainersService _containersService;
	private readonly Trader _trader;

	private string getOpenSignal(Container container, double price)
	{
		if (container.OpenStraddle != null)
			return "Already have open straddle";

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

		var baseCall = _connector
			.RequestCall(container.ParentInstrument, basestrike, optionclass.ExpirationDate);
		var closureCall = _connector
			.RequestCall(container.ParentInstrument, closureCallStike, optionclass.ExpirationDate);

		var basePut = _connector
			.RequestPut(container.ParentInstrument, basestrike, optionclass.ExpirationDate);
		var closurePut = _connector
			.RequestPut(container.ParentInstrument, closurePutStrike, optionclass.ExpirationDate);

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
	private string getCloseSignal(Container container)
	{
		if (container == null) return "No container";
		string message = string.Empty;
		if (container.CurrencyOpenPnl >= container.StraddleTargetPnl)
		{
			message = $"Current Pnl: {container.CurrencyOpenPnl}. TargetPnl: {container.StraddleTargetPnl}. Closing by profit";
			container.Close();
			return message;
		}
		if (container.OpenStraddle?.CreatedTime > container.ApproximateCloseDate)
		{
			message = $"Opened at: {container.OpenStraddle?.CreatedTime}. " +
                $"ApproximateCloseDate: {container.ApproximateCloseDate}. Closing by close date";

			container.Close();
			return message;
		}
		if (container.OpenStraddle?.IsDone() is false)
		{
			message = $"Страддл не успел открывться. Закрываю!";
			container.Close();
			return message;
        }

        message = $"Current Pnl: {container.CurrencyOpenPnl}. TargetPnl: {container.StraddleTargetPnl}.\n" +
            $"Opened at: {container.OpenStraddle?.CreatedTime}." +
            $"ApproximateCloseDate: {container.ApproximateCloseDate}.";

        return message;
	}
	public McAPIController(ILogger<McAPIController> logger, IConnector connector, ContainersService containersService, Trader trader)
	{
		_logger = logger;
		_connector = connector;
		_containersService = containersService;
		_trader = trader;
	}

	[HttpGet]
	public IActionResult Get(string symbol, double price, string account, string type)
	{
		_logger.LogInformation($"SIGNAL::symbol:{symbol}::price:{price}::account:{account}::type:{type}");
		var container = _trader.GetContainer(symbol, account);
		if (container == null)
		{
			_logger.LogInformation("No container in trade.");
			return Ok();
		}
		type = type.Trim().ToUpper();
		if (type == "OPEN")
		{
			_logger.LogInformation(getOpenSignal(container, price));
        }
		else 
		{
			_logger.LogInformation(getCloseSignal(container));
		}

		return Ok();
	}
}
