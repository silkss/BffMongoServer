namespace Strategies.Base;

using Connectors;
using Common.Types.Base;
using Common.Types.Orders;
using Common.Types.Instruments;
using Common.Types.Orders.Asbstractions;
using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

public class OptionTradeUnit : IOrderHolder
{
    
    private Directions getCloseDirection() => Direction == Directions.Buy ? Directions.Sell : Directions.Buy;
    private int getTradableVolume() => Volume - Math.Abs(Position);
    private Order createOpenOrder(string account) => new Order(this, account)
    {
        Quantity = getTradableVolume(),
        LimitPrice = Instrument.TradablePrice(Direction),
        Direction = Direction,
    };
    private Order createCloseOrder(string account) => new Order(this, account)
    {
        Quantity = Math.Abs(Position),
        LimitPrice = Instrument.TradablePrice(getCloseDirection()),
        Direction = getCloseDirection(),
    };
    private void createAndSendOrder(bool isOpen, IConnector connector, string account, int priceShift)
    {
        if (isOpen) OpenOrder = createOpenOrder(account);
        else OpenOrder = createCloseOrder(account);

        lock (Orders)
        {
            Orders.Add(OpenOrder);
        }

        connector.SendLimitOrder(Instrument, OpenOrder, priceShift, true);
    }
    private void updateTradeInfo() =>
        (Position, ClosedPnlWithoutCommission, CommissionCurrency, EnterPriceWithCommission) = StrategyHelper.GetPosition(Orders, Direction, Instrument.Multiplier);

    public OptionTradeUnit() { }
    public OptionTradeUnit(Instrument instrument, Directions directions, int volume, TradeLogic logic)
    {
        Instrument = instrument;
        Direction = directions;
        Volume = volume;
        Logic = logic;
    }
    public Instrument Instrument { get; set; }
    public Directions Direction { get; set; }
    public List<Order> Orders { get; set; } = new();
    public Order? OpenOrder { get; set; }
    public TradeLogic Logic { get; set; }
    public int Volume { get; set; }

    [BsonIgnore] public decimal EnterPriceWithCommission { get; private set; }
    [BsonIgnore] public int Position { get; private set; }
    [BsonIgnore] public decimal CommissionCurrency { get; private set; }
    [BsonIgnore] public decimal ClosedPnlWithoutCommission { get; private set; }

    public decimal GetOpenPnlWithoutCommision() => Instrument.TheorPrice == 0 ?
        0m :
        Direction switch
        {
            Directions.Buy => ClosedPnlWithoutCommission  + Instrument.TheorPrice, //при покупке СlosePnl отрицательная.
            Directions.Sell => ClosedPnlWithoutCommission - Instrument.TheorPrice,
            _ => 0m
        };
    public decimal GetCurrencyPnlWithCommission()
    {
        var pnl = Logic == TradeLogic.Open ? GetOpenPnlWithoutCommision() : ClosedPnlWithoutCommission;
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
        updateTradeInfo();
    }
    public void Work(IConnector connector, string account, int priceShift)
    {
        updateTradeInfo();
        switch (Logic)
        {
            case TradeLogic.Open when OpenOrder == null:
                if (Math.Abs(Position) == Volume) break;
                if (Instrument.TradablePrice(Direction) <= 0m) break;
                createAndSendOrder(true, connector, account, priceShift);
                break;
            case TradeLogic.Open when OpenOrder != null:
                if (!connector.IsOrderOpen(OpenOrder))
                {
                    OpenOrder = null;
                    break;
                }
                if (StrategyHelper.OrderPriceOutBound(OpenOrder, Instrument.TradablePrice(Direction), Instrument.MinTick))
                {
                    connector.CancelOrder(OpenOrder);
                }
                break;
            case TradeLogic.Close when OpenOrder == null:
                if (Position == 0) break;
                createAndSendOrder(false, connector, account, priceShift);
                break;
            case TradeLogic.Close when OpenOrder != null:
                if (!connector.IsOrderOpen(OpenOrder))
                {
                    OpenOrder = null;
                    break;
                }
                if (StrategyHelper.OrderPriceOutBound(OpenOrder, Instrument.TradablePrice(Direction), Instrument.MinTick))
                {
                    connector.CancelOrder(OpenOrder);
                }

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
