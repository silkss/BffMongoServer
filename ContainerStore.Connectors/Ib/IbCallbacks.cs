using ContainerStore.Connectors.Converters.Ib;
using ContainerStore.Connectors.Ib.Caches;
using ContainerStore.Connectors.Models;
using ContainerStore.Data.Models;
using IBApi;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace ContainerStore.Connectors.Ib;

internal class IbCallbacks : DefaultEWrapper
{
	private readonly ILogger<IbConnector> _logger;
	private readonly RequestInstrumentCache _requestInstrument;


	private void onPriceChanged(PriceChangedEventArgs args)
	{
		var handler = PriceChange;
		if (handler != null)
		{
			handler(this, args);
		}
	}
	public event EventHandler<PriceChangedEventArgs> PriceChange;
	public int NextOrderId { get; set; }

    public IbCallbacks(ILogger<IbConnector> logger, RequestInstrumentCache requestInstrument)
	{
		_logger = logger;
		_requestInstrument = requestInstrument;
	}
    public List<Account> Accounts { get; } = new();
	public override void contractDetails(int reqId, ContractDetails contractDetails)
	{
		_requestInstrument.Add(reqId, contractDetails.ToInstrument());
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
