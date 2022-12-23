using Common.Types.Base;
using Common.Types.Orders;
using Common.Types.Instruments;
using Connectors;
using Notifier;
using Strategies.Helpers;
using Strategies.Settings;
using System;
using System.Collections.Generic;
using Common.Types.Orders.Asbstractions;

namespace Strategies.Strategies.Depend.Base;

public abstract class OptionStrategy : IOrderHolder
{
    protected Directions getCloseDirection() => Direction == Directions.Buy
        ? Directions.Sell
        : Directions.Buy;
    protected int getTradableVolume()
    {
        (var pos, _) = StrategyHelper.GetPosition(Orders);
        return Volume - Math.Abs(pos);
    }
    protected Order createOpenOrder(MainSettings settings, decimal orderPrice) => new Order(this, settings.Account)
    {
        Quantity = getTradableVolume(),
        LimitPrice = orderPrice == 0 ? Instrument.TradablePrice(Direction) : orderPrice,
        Direction = Direction,
    };
    protected Order createCloseOrder(MainSettings settings) => new Order(this, settings.Account)
    {
        Quantity = Math.Abs(StrategyHelper.GetPosition(Orders).pos),
        LimitPrice = Instrument.TradablePrice(getCloseDirection()),
        Direction = getCloseDirection(),
    };
    protected void createAndSendOrder(bool isOpen, IConnector connector, MainSettings settings, decimal orderPrice = 0)
    {
        if (isOpen)
            OpenOrder = createOpenOrder(settings, orderPrice);
        else
            OpenOrder = createCloseOrder(settings);
        lock (Orders)
        {
            Orders.Add(OpenOrder);
        }
        if (orderPrice == 0)
        {
            connector.SendLimitOrder(Instrument, OpenOrder, settings.OrderPriceShift, true);
        }
        else
        {
            connector.SendLimitOrder(Instrument, OpenOrder, 0, true);
        }
    }

    public Instrument Instrument { get; set; }
    public Directions Direction { get; set; }
    public List<Order> Orders { get; set; } = new();
    public Order? OpenOrder { get; set; }
    public TradeLogic Logic { get; set; }
    public int Volume { get; set; }
    public virtual void Start(IConnector connector)
    {
        if (Instrument.LastTradeDate < DateTime.Now) return;
        connector.RequestMarketData(Instrument);
        connector.ReqMarketRule(Instrument.MarketRuleId);
    }
    public virtual void Work(IConnector connector, IBffLogger notifier, MainSettings mainSettings, decimal orderPrice = 0m)
    {
        switch (Logic)
        {
            case TradeLogic.Open when OpenOrder == null:
                if (StrategyHelper.Opened(Orders, Volume)) break;
                createAndSendOrder(true, connector, mainSettings, orderPrice);
                break;
            case TradeLogic.Open when OpenOrder != null:
                if (!connector.IsOrderOpen(OpenOrder))
                {
                    OpenOrder = null;
                    break;
                }
                if (StrategyHelper.OrderPriceOutBound(OpenOrder, Instrument.TradablePrice(Direction), mainSettings))
                    connector.CancelOrder(OpenOrder);
                break;
            case TradeLogic.Close when OpenOrder == null:
                if (StrategyHelper.Closed(Orders)) break;
                createAndSendOrder(false, connector, mainSettings, orderPrice);
                break;
            case TradeLogic.Close when OpenOrder != null:
                if (!connector.IsOrderOpen(OpenOrder))
                {
                    OpenOrder = null;
                    break;
                }
                if (StrategyHelper.OrderPriceOutBound(OpenOrder, Instrument.TradablePrice(Direction), mainSettings))
                    connector.CancelOrder(OpenOrder);
  
                break;
            default:
                break;

        }
    }

    #region IOrderHolder
    public virtual void OnOrderCancelled(int brokerId)
    {
        throw new System.NotImplementedException();
    }

    public virtual void OnOrderFilled(int brokerId)
    {
        throw new System.NotImplementedException();
    }

    public virtual void OnSubmitted(int brokerId)
    {
        throw new System.NotImplementedException();
    }
    #endregion
}
