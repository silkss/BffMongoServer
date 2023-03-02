using Common.Events;
using Common.Helpers;
using Common.Types.Instruments;
using Connectors.Info;
using Connectors.Ib.Caches;
using Connectors.Converters.Ib;
using IBApi;
using System;
using System.Collections.Generic;
using System.Globalization;
using Common.Types.Base;
using Microsoft.Extensions.Logging;

namespace Connectors.Ib;

internal class IbCallbacks : DefaultEWrapper
{
	private readonly ILogger<IbConnector> _logger;
	private readonly RequestInstrumentCache _requestInstrument;
	private readonly Dictionary<int, OptionChain> _optionChains;
	private readonly OpenOrdersCache _openOrdersCache;
	private readonly Dictionary<int, List<PriceBorder>> _marketRules;
	private readonly ConnectorInfo _connectionInfo;
    
    private void onPriceChanged(PriceChangedEventArgs args)
	{
		var handler = PriceChange;
		if (handler != null)
		{
			handler(this, args);
		}
	}
	public event EventHandler<PriceChangedEventArgs> PriceChange = delegate { };
    public event Action<bool> ConnectionChanged = delegate { };
	private int _nextOrderId;
    public int NextOrderId 
	{ 
		get => _nextOrderId++;
		private set => _nextOrderId = value;
	}
    public IbCallbacks(
        ILogger<IbConnector> logger, RequestInstrumentCache requestInstrument, 
		Dictionary<int, OptionChain> optionChains, OpenOrdersCache openOrdersCache,
		Dictionary<int, List<PriceBorder>> marketRules, ConnectorInfo connectionInfo)
	{
		_logger = logger;
		_requestInstrument = requestInstrument;
		_optionChains = optionChains;
		_openOrdersCache = openOrdersCache;
		_marketRules = marketRules;
		_connectionInfo = connectionInfo;
	}
    public event Action LostConnection = delegate { };

	public override void contractDetails(int reqId, IBApi.ContractDetails contractDetails)
	{
		_requestInstrument.Add(reqId, contractDetails.ToInstrument());
	}
	public override void marketRule(int marketRuleId, IBApi.PriceIncrement[] priceIncrements)
	{
		if (_marketRules.ContainsKey(marketRuleId))
		{
			_marketRules[marketRuleId].Clear();
		}
		else
		{
			_marketRules[marketRuleId] = new List<PriceBorder>();
		}

		foreach (var inc in priceIncrements)
		{
			var price_border = new PriceBorder
			{
				LowEdge = MathHelper.ConvertDoubleToDecimal(inc.LowEdge),
				Incriment = MathHelper.ConvertDoubleToDecimal(inc.Increment)
			};
			_marketRules[marketRuleId].Add(price_border);
		}
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
            case IBApi.TickType.ASK:
            case IBApi.TickType.ASK_OPTION:
            case IBApi.TickType.DELAYED_ASK:
            case IBApi.TickType.DELAYED_ASK_OPTION:
                var ask = new PriceChangedEventArgs();
                ask.TickerId = tickerId;
                ask.Price = Convert.ToDecimal(price);
                ask.Tick = Tick.Ask;
                onPriceChanged(ask);
                break;
            case IBApi.TickType.BID:
            case IBApi.TickType.DELAYED_BID:
            case IBApi.TickType.BID_OPTION:
            case IBApi.TickType.DELAYED_BID_OPTION:
                var bid = new PriceChangedEventArgs();
                bid.TickerId = tickerId;
                bid.Price = Convert.ToDecimal(price);
                bid.Tick = Tick.Bid;
                onPriceChanged(bid);
                break;
            case IBApi.TickType.LAST:
            case IBApi.TickType.DELAYED_LAST:
            case IBApi.TickType.LAST_OPTION:
            case IBApi.TickType.DELAYED_LAST_OPTION:
                var last = new PriceChangedEventArgs();
                last.TickerId = tickerId;
                last.Price = Convert.ToDecimal(price);
                last.Tick = Tick.Last;
                onPriceChanged(last);
                break;
        }
	}
	public override void tickOptionComputation(int tickerId, int field, int tickAttrib, double impliedVolatility, double delta, double optPrice, double pvDividend, double gamma, double vega, double theta, double undPrice)
	{
		if (field != IBApi.TickType.MODEL_OPTION &&
			field != IBApi.TickType.DELAYED_MODEL_OPTION)
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
        _connectionInfo.Accounts.Clear();
		foreach (var account in accountsList.Trim().Split(','))
		{
            _connectionInfo.Accounts.Add(account);
		}
	}
	public override void openOrder(int orderId, IBApi.Contract contract, IBApi.Order order, IBApi.OrderState orderState)
	{
		if (orderState.Commission == double.MaxValue) return;
		if (_openOrdersCache.GetById(orderId) is Common.Types.Orders.Order openorder)
		{
			if (openorder.FilledQuantity == openorder.Quantity)
			{
				openorder.Commission = MathHelper.ConvertDoubleToDecimal(orderState.Commission);
				openorder.Filled();
				_openOrdersCache.Remove(openorder);
			}
		}
    }
	public override void orderStatus(int orderId, string status, decimal filled, decimal remaining, double avgFillPrice, int permId, int parentId, double lastFillPrice, int clientId, string whyHeld, double mktCapPrice)
	{
        if (_openOrdersCache.GetById(orderId) is Common.Types.Orders.Order openorder)
        {
            openorder.AvgFilledPrice = MathHelper.ConvertDoubleToDecimal(avgFillPrice);
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
            case 200: // что-то не то с запросом инструмента.
                _requestInstrument.Add(id, null);
                _requestInstrument.ReceivedSignal();
				_logger.LogError("Чтото не так с запросом инструмента.CODE:{errorCode}\nMESSAGE:{errorMsg}",errorCode, errorMsg);
                break;
            case 2106:  // Connected!
				_connectionInfo.IsConnected = true;
				ConnectionChanged?.Invoke(true);
				break;
            case 504:   // NotConnected
				_requestInstrument.ReceivedSignal(); // если вдруг ктото ждет инструмент.
				_logger.LogCritical("Not connected!");
				_connectionInfo.IsConnected = false;
                ConnectionChanged?.Invoke(false);
                break;
            case 110:	// wrong order price.
			case 10147: // не найден ордер для отмены. Будем все равно имитировать что его отменили. Хотя, скорей всего, он исполнился.
			case 10148: // Order already cancelled
            case 201:	// ордер отклонен
            case 202:	// someone cancelled order
            case 512:
			case 321: // Зачемто послан ордер с нулевым объемом!
                if (_openOrdersCache.GetById(id) is Common.Types.Orders.Order canceledorder)
                {
					_logger.LogError("Something wrong with order: {errorMsg}", errorMsg);

					_openOrdersCache.Remove(canceledorder);
                    canceledorder.Canceled();
                }
                break;
            default:
                _logger.LogError("ID:{id} : ERROR_CODE:{errorCode} : MESSAGE:{errorMsg}", id, errorCode, errorMsg);
				break;
        }
	}
	public override void error(string str)
	{
		_logger.LogError(str);
	}
}
