namespace Strategies.TradeUnits;

using Connectors;
using Strategies.Helpers;
using Strategies.Settings;
using Common.Types.Base;
using Common.Types.Orders;
using Common.Types.Instruments;
using Common.Types.Orders.Asbstractions;
using System;
using System.Collections.Generic;

public class OptionTradeUnit : IOrderHolder
{
    private Directions getCloseDirection() => Direction == Directions.Buy
        ? Directions.Sell
        : Directions.Buy;
    private int getTradableVolume()
    {
        (var pos, _, _) = StrategyHelper.GetPosition(Orders);
        return Volume - Math.Abs(pos);
    }
    private Order createOpenOrder(ContainerSettings settings) => new Order(this, settings.Account)
    {
        Quantity = getTradableVolume(),
        LimitPrice = Instrument.TradablePrice(Direction),
        Direction = Direction,
    };
    private Order createCloseOrder(ContainerSettings settings) => new Order(this, settings.Account)
    {
        Quantity = Math.Abs(StrategyHelper.GetPosition(Orders).pos),
        LimitPrice = Instrument.TradablePrice(getCloseDirection()),
        Direction = getCloseDirection(),
    };
    private void createAndSendOrder(bool isOpen, IConnector connector, ContainerSettings containerSettings)
    {
        if (isOpen)
            OpenOrder = createOpenOrder(containerSettings);
        else
            OpenOrder = createCloseOrder(containerSettings);

        lock (Orders)
        {
            Orders.Add(OpenOrder);
        }

        connector.SendLimitOrder(Instrument, OpenOrder, containerSettings.OrderPriceShift, true);
    }
    private void updateTradeInfo() =>
        (Position, ClosedPnl, CommissionCurrency) = StrategyHelper.GetPosition(Orders);
    
    public OptionTradeUnit() => updateTradeInfo();
    public OptionTradeUnit(Instrument instrument, Directions directions, int volume)
    {
        Instrument = instrument;
        Direction = directions;
        Volume = volume;
    }
    public Instrument Instrument { get; set; }
    public Directions Direction { get; set; }
    public List<Order> Orders { get; set; } = new();
    public Order? OpenOrder { get; set; }
    public TradeLogic Logic { get; set; }
    public int Volume { get; set; }
    public int Position { get; private set; }
    public decimal CommissionCurrency { get; private set; }
    public decimal ClosedPnl { get; private set; }
    public decimal OpenPnl => Instrument.TheorPrice == 0 ?
        0m :
        Direction switch
        {
            Directions.Buy => ClosedPnl + Instrument.TheorPrice, //при покупке СlosePnl отрицательная.
            Directions.Sell => ClosedPnl -  Instrument.TheorPrice,
            _ => 0m
        };
    public decimal GetCurrencyPnl()
    {
        var pnl = Logic == TradeLogic.Open ? OpenPnl : ClosedPnl;
        return pnl * Instrument.Multiplier - CommissionCurrency;
    }
    public bool IsDone() => Logic switch
    {
        TradeLogic.Open => Math.Abs(Position) == Volume,
        TradeLogic.Close => Position == 0,
        _ => throw new NotSupportedException("Unknow TradeLogic!")
    };
    public void Start(IConnector connector)
    {
        if (Instrument.LastTradeDate < DateTime.Now) return;
        connector.RequestMarketData(Instrument);
        connector.ReqMarketRule(Instrument.MarketRuleId);
    }
    public void Work(IConnector connector, ContainerSettings containerSettings)
    {
        updateTradeInfo();
        switch (Logic)
        {
            case TradeLogic.Open when OpenOrder == null:
                if (Math.Abs(Position) == Volume) break;
                if (Instrument.TradablePrice(Direction) <= 0m) break;
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
                if (Position == 0) break;
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
    public void Stop(IConnector connector) => connector.CancelOrder(OpenOrder);
    public void Close()
    {
        Logic = TradeLogic.Close;
    }

    #region IOrderHolder
    public virtual void OnOrderCancelled(int brokerId)
    {
        if (OpenOrder == null) return;
        if (brokerId != OpenOrder.BrokerId) return;
        OpenOrder = null;
    }

    public virtual void OnOrderFilled(int brokerId)
    {
        if (OpenOrder == null) return;
        if (brokerId != OpenOrder.BrokerId) return;
        OpenOrder = null;
    }

    public virtual void OnSubmitted(int brokerId)
    {
        throw new NotImplementedException();
    }
    #endregion
}
