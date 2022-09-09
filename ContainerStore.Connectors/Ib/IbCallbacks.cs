using ContainerStore.Common.Enums;
using ContainerStore.Common.Helpers;
using ContainerStore.Connectors.Converters.Ib;
using ContainerStore.Connectors.Ib.Caches;
using ContainerStore.Data.Models;
using ContainerStore.Data.Models.Accounts;
using ContainerStore.Data.Models.Instruments;
using ContainerStore.Data.Models.Transactions;
using IBApi;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ContainerStore.Connectors.Ib;

internal class IbCallbacks : DefaultEWrapper
{
	private readonly ILogger<IbConnector> _logger;
	private readonly RequestInstrumentCache _requestInstrument;
	private readonly Dictionary<int, OptionChain> _optionChains;
	private readonly OpenOrdersCache _openOrdersCache;

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

    public IbCallbacks(
		ILogger<IbConnector> logger, RequestInstrumentCache requestInstrument, 
		Dictionary<int, OptionChain> optionChains, OpenOrdersCache openOrdersCache)
	{
		_logger = logger;
		_requestInstrument = requestInstrument;
		_optionChains = optionChains;
		_openOrdersCache = openOrdersCache;
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
                strikes));
		}
	}
	public override void securityDefinitionOptionParameterEnd(int reqId)
	{
		_optionChains[reqId].RefreshRequestTime();
	}
	public override void tickPrice(int tickerId, int field, double price, TickAttrib attribs)
	{
		//_logger.LogInformation($"{tickerId}:{TickType.getField(field)}:\t{price}");
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
	public override void tickOptionComputation(int tickerId, int field, int tickAttrib, double impliedVolatility, double delta, double optPrice, double pvDividend, double gamma, double vega, double theta, double undPrice)
	{
		if (field != TickType.MODEL_OPTION &&
			field != TickType.DELAYED_MODEL_OPTION)
			return;

		if (optPrice != double.MaxValue)
		{
			var model = new PriceChangedEventArgs();

			model.TickerId = tickerId;
			model.Price = Convert.ToDecimal(optPrice);
			model.Tick = Tick.TheorPrice;

			onPriceChanged(model);
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
	public override void openOrder(int orderId, Contract contract, Order order, OrderState orderState)
	{
		if (orderState.Commission == double.MaxValue) return;
		if (_openOrdersCache.GetById(orderId) is Transaction openorder)
		{
			if (openorder.FilledQuantity == openorder.Quantity)
			{
				openorder.Commission = Helper.ConvertDoubleToDecimal(orderState.Commission);
				openorder.Filled();
				_openOrdersCache.Remove(openorder);
			}
		}
    }
	public override void orderStatus(int orderId, string status, decimal filled, decimal remaining, double avgFillPrice, int permId, int parentId, double lastFillPrice, int clientId, string whyHeld, double mktCapPrice)
	{
        if (_openOrdersCache.GetById(orderId) is Transaction openorder)
        {
            openorder.AvgFilledPrice = Helper.ConvertDoubleToDecimal(avgFillPrice);
            openorder.FilledQuantity = (int)filled;

            if (openorder.Status == "Submitted")
            {
                openorder.Submitted();
            }
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
			case 110: // wrong order price.
				break; 
            case 200: // что-то не то с запросом инструмента.
                _requestInstrument.Add(id, null);
                _requestInstrument.ReceivedSignal();
				_logger.LogError($"Чтото не так с запросом инструмента.CODE:{errorCode}\nMESSAGE:{errorMsg}");
                break;
            case 201: //Ордер отклонен
            case 202: // someone cancelled order
            case 512:
                if (_openOrdersCache.GetById(id) is Transaction canceledorder)
                {
                    _openOrdersCache.Remove(canceledorder);
                    canceledorder.Canceled();
                }
                break;
            default:
                _logger.LogError($"ID:{id} : ERROR_CODE:{errorCode} : MESSAGE:{errorMsg}");
				break;
        }
	}
	public override void error(string str)
	{
		_logger.LogError(str);
	}
}
