using ContainerStore.Connectors.Converters.Ib;
using ContainerStore.Connectors.Ib.Caches;
using ContainerStore.Data.Models;
using ContainerStore.Data.Models.Accounts;
using ContainerStore.Data.Models.Instruments;
using IBApi;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ContainerStore.Connectors.Ib;

internal class IbCallbacks : DefaultEWrapper
{
	private readonly ILogger<IbConnector> _logger;
	private readonly RequestInstrumentCache _requestInstrument;
	private readonly Dictionary<int, OptionChain> _optionChains;

	private void onPriceChanged(PriceChangedEventArgs args)
	{
		var handler = PriceChange;
		if (handler != null)
		{
			handler(this, args);
		}
	}
	public event EventHandler<PriceChangedEventArgs> PriceChange = delegate { };
	public int NextOrderId { get; set; }

    public IbCallbacks(ILogger<IbConnector> logger, RequestInstrumentCache requestInstrument, Dictionary<int, OptionChain> optionChains)
	{
		_logger = logger;
		_requestInstrument = requestInstrument;
		_optionChains = optionChains;
	}
    public List<Account> Accounts { get; } = new();
	public override void contractDetails(int reqId, ContractDetails contractDetails)
	{
		_requestInstrument.Add(reqId, contractDetails.ToInstrument());
	}
	public override void securityDefinitionOptionParameter(int reqId, string exchange, int underlyingConId, string tradingClass, string multiplier, HashSet<string> expirations, HashSet<double> strikes)
	{
		foreach (var exp in expirations)
		{
			_optionChains[reqId].AddTradingClass(new OptionTradingClass(
                underlyingConId, exchange, tradingClass, int.Parse(multiplier),
                DateTime.ParseExact(exp, "yyyyMMdd", CultureInfo.CurrentCulture),
                strikes.Select(s => Convert.ToDecimal(s)).ToList()));
		}
	}
	public override void securityDefinitionOptionParameterEnd(int reqId)
	{
		_optionChains[reqId].RefreshRequestTime();
	}
	public override void tickPrice(int tickerId, int field, double price, TickAttrib attribs)
	{
		_logger.LogInformation($"{tickerId}:{TickType.getField(field)}:\t{price}");
		switch (field)
        {
            case TickType.ASK:
            case TickType.ASK_OPTION:
            case TickType.DELAYED_ASK:
            case TickType.DELAYED_ASK_OPTION:
                var ask = new PriceChangedEventArgs();
                ask.TickerId = tickerId;
                ask.Price = Convert.ToDecimal(price);
                ask.Tick = Common.Enums.Tick.Ask;
                onPriceChanged(ask);
                break;
            case TickType.BID:
            case TickType.DELAYED_BID:
            case TickType.BID_OPTION:
            case TickType.DELAYED_BID_OPTION:
                var bid = new PriceChangedEventArgs();
                bid.TickerId = tickerId;
                bid.Price = Convert.ToDecimal(price);
                bid.Tick = Common.Enums.Tick.Bid;
                onPriceChanged(bid);
                break;
            case TickType.LAST:
            case TickType.DELAYED_LAST:
            case TickType.LAST_OPTION:
            case TickType.DELAYED_LAST_OPTION:
                var last = new PriceChangedEventArgs();
                last.TickerId = tickerId;
                last.Price = Convert.ToDecimal(price);
                last.Tick = Common.Enums.Tick.Last;
                onPriceChanged(last);
                break;
        }
	}
	public override void nextValidId(int orderId)
	{
		NextOrderId = orderId;
	}
	public override void managedAccounts(string accountsList)
	{
        Accounts.Clear();
		foreach (var account in accountsList.Trim().Split(','))
		{
            Accounts.Add(new Account { Name = account });
		}
	}
	public override void error(Exception e)
	{
		_logger.LogCritical(e.Message);
	}
	public override void error(int id, int errorCode, string errorMsg, string advancedOrderRejectJson)
	{
		switch (errorCode)
		{
            case 200: // что-то не то с запросом инструмента.
                _requestInstrument.Add(id, null);
                _requestInstrument.ReceivedSignal();
				_logger.LogError($"Чтото не так с запросом инструмента.CODE:{errorCode}\nMESSAGE:{errorMsg}");
                break;
			default:
                _logger.LogError($"ERROR_CODE:{errorCode} : MESSAGE:{errorMsg}");
				break;
        }
	}
	public override void error(string str)
	{
		_logger.LogError(str);
	}
}
