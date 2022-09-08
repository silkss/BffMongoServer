﻿using ContainerStore.Connectors;
using ContainerStore.Data.Models;
using ContainerStore.Data.Models.TradeUnits;
using ContainerStore.Traders.Base;
using ContainerStore.WebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
		var container = _trader.GetContainer(symbol, account);
		if (container == null) return Ok();
		type = type.Trim().ToUpper();
		if (type == "OPEN")
		{
			var optionclass = _connector
				.GetOptionTradingClass(container.ParentInstrument.Id, container.GetApproximateExpirationDate());
			if (optionclass == null)
			{
				return Ok();
			}
			var basestrike = optionclass.Strikes.MinBy(s => Math.Abs(s - price));
			var basestrikeIdx = optionclass.Strikes.FindIndex(s => s == basestrike);
			var closureCallStike = optionclass.Strikes[basestrikeIdx + container.ClosureStrikeStep];
			var closurePutStrike = optionclass.Strikes[basestrikeIdx - container.ClosureStrikeStep];

			var baseCall = _connector
				.RequestCall(container.ParentInstrument, basestrike, optionclass.ExpirationDate);
			var basePut = _connector
				.RequestPut(container.ParentInstrument, basestrike, optionclass.ExpirationDate);
			if (baseCall is null)
			{
				_logger.LogError($"не удалось запросить Call для иснтумента:\n" +
					$"parentId: {container.ParentInstrument.Id}. Strike:{basestrike}. ExpDate: {optionclass.ExpirationDate}.");
				return Ok();
			}
			if (basePut is null)
            {
                _logger.LogError($"не удалось запросить Put для иснтумента:\n" +
                    $"parentId: {container.ParentInstrument.Id}. Strike:{basestrike}. ExpDate: {optionclass.ExpirationDate}.");
                return Ok();
            }

			_connector
				.RequestMarketData(baseCall)
				.RequestMarketData(basePut);

			container.AddStraddle(new Straddle(baseCall, basePut));
            var puLegInsturment = _connector.RequestPut(container.ParentInstrument, basestrike, optionclass.ExpirationDate);
        }

		return Ok();
	}
}
