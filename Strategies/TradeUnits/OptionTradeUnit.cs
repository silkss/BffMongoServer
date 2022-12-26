using Notifier;
using Connectors;
using Strategies.Helpers;
using Strategies.Settings;
using Common.Types.Base;
using Common.Types.Orders;
using Common.Types.Instruments;
using Common.Types.Orders.Asbstractions;
using System;
using System.Collections.Generic;

namespace Strategies.TradeUnits;

public class OptionTradeUnit : IOrderHolder
{
    protected Directions getCloseDirection() => Direction == Directions.Buy
        ? Directions.Sell
        : Directions.Buy;
    protected int getTradableVolume()
    {
        (var pos, _) = StrategyHelper.GetPosition(Orders);
        return Volume - Math.Abs(pos);
    }
    protected Order createOpenOrder(ContainerSettings settings) => new Order(this, settings.Account)
    {
        Quantity = getTradableVolume(),
        LimitPrice = Instrument.TradablePrice(Direction),
        Direction = Direction,
    };
    protected Order createCloseOrder(ContainerSettings settings) => new Order(this, settings.Account)
    {
        Quantity = Math.Abs(StrategyHelper.GetPosition(Orders).pos),
        LimitPrice = Instrument.TradablePrice(getCloseDirection()),
        Direction = getCloseDirection(),
    };
    protected void createAndSendOrder(bool isOpen, IConnector connector, ContainerSettings containerSettings)
    {
        if (isOpen)
            OpenOrder = createOpenOrder(containerSettings);
        else
            OpenOrder = createCloseOrder(containerSettings);
        lock (Orders)
        {
            Orders.Add(OpenOrder);
        }

        connector.SendLimitOrder(Instrument, OpenOrder,2, true);
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
    public virtual void Work(IConnector connector, ContainerSettings containerSettings)
    {
        switch (Logic)
        {
            case TradeLogic.Open when OpenOrder == null:
                if (StrategyHelper.Opened(Orders, Volume)) break;
                createAndSendOrder(true, connector, containerSettings);
                break;
            case TradeLogic.Open when OpenOrder != null:
                if (!connector.IsOrderOpen(OpenOrder))
                {
                    OpenOrder = null;
                    break;
                }
                if (StrategyHelper.OrderPriceOutBound(OpenOrder, Instrument.TradablePrice(Direction), containerSettings))
                    connector.CancelOrder(OpenOrder);
                break;
            case TradeLogic.Close when OpenOrder == null:
                if (StrategyHelper.Closed(Orders)) break;
                createAndSendOrder(false, connector, containerSettings);
                break;
            case TradeLogic.Close when OpenOrder != null:
                if (!connector.IsOrderOpen(OpenOrder))
                {
                    OpenOrder = null;
                    break;
                }
                if (StrategyHelper.OrderPriceOutBound(OpenOrder, Instrument.TradablePrice(Direction), containerSettings))
                    connector.CancelOrder(OpenOrder);

                break;
            default:
                break;

        }
    }

    #region IOrderHolder
    public virtual void OnOrderCancelled(int brokerId)
    {
        throw new NotImplementedException();
    }

    public virtual void OnOrderFilled(int brokerId)
    {
        throw new NotImplementedException();
    }

    public virtual void OnSubmitted(int brokerId)
    {
        throw new NotImplementedException();
    }
    #endregion
}
