namespace Traders.Strategies.Base;

using Connectors;
using Common.Types.Base;
using Common.Types.Orders;
using Common.Types.Instruments;
using Common.Types.Orders.Asbstractions;
using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using Microsoft.Extensions.Logging;

public class OptionTradeUnit : IOrderHolder
{

    private Directions getCloseDirection() => Direction == Directions.Buy ? Directions.Sell : Directions.Buy;
    private int getTradableVolume() => Volume - Math.Abs(Position);
    private Order createOpenOrder(string account) => new Order(this, account)
    {
        Quantity = getTradableVolume(),
        LimitPrice = Instrument.TradablePrice(Direction),
        Account = account,
        Direction = Direction,
    };
    private Order createCloseOrder(string account) => new Order(this, account)
    {
        Quantity = Math.Abs(Position),
        LimitPrice = Instrument.GetBidAskTradablePrice(getCloseDirection()),
        Account = account,
        Direction = getCloseDirection(),
    };
    private void createAndSendOrder(bool isOpen, IConnector connector, string account, int priceShift = 0)
    {
        if (isOpen) OpenOrder = createOpenOrder(account);
        else OpenOrder = createCloseOrder(account);

        lock (Orders)
        {
            Orders.Add(OpenOrder);
        }

        connector.SendLimitOrder(Instrument, OpenOrder, priceShift, !isOpen);
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

    public bool HasTradeDate() => Logic switch {
        TradeLogic.Open => Instrument.TradablePrice(Direction) > 0m,
        TradeLogic.Close => Instrument.TradablePrice(getCloseDirection()) > 0m,
        _ =>throw new NotSupportedException("Unknown trade logic!")
    };

    [BsonIgnore] public decimal EnterPriceWithCommission { get; private set; }
    [BsonIgnore] public int Position { get; private set; }
    [BsonIgnore] public decimal CommissionCurrency { get; private set; }
    [BsonIgnore] public decimal ClosedPnlWithoutCommission { get; private set; }
    public decimal GetBidAskOpenPnlWithoutCommission() => Direction switch
    {
        Directions.Buy => Instrument.Ask == 0m ? 0m :
                ClosedPnlWithoutCommission == 0m ? 0m :
                    ClosedPnlWithoutCommission + Instrument.Ask,
        Directions.Sell => Instrument.Bid == 0m ? 0m :
                ClosedPnlWithoutCommission == 0m ? 0m :
                    ClosedPnlWithoutCommission - Instrument.Bid,
        _ => 0m
    };
    public decimal GetCurrencyBidAskPnlWithCommission()
    {
        var pnl = Logic == TradeLogic.Open ? GetBidAskOpenPnlWithoutCommission() : ClosedPnlWithoutCommission;
        return pnl * Instrument.Multiplier - CommissionCurrency;
    }
    public decimal GetTheorOpenPnlWithoutCommision() => Instrument.TheorPrice <= 0 ? 0m :
        Direction switch
        {
            Directions.Buy => ClosedPnlWithoutCommission == 0m ? 
                0m : 
                ClosedPnlWithoutCommission + Instrument.TheorPrice, //при покупке СlosePnl отрицательная.
            Directions.Sell => ClosedPnlWithoutCommission == 0m ? 
                0m : 
                ClosedPnlWithoutCommission - Instrument.TheorPrice,
            _ => 0m
        };
    public decimal GetCurrencyTheorPnlWithCommission()
    {
        var pnl = Logic == TradeLogic.Open ? GetTheorOpenPnlWithoutCommision() : ClosedPnlWithoutCommission;
        return pnl * Instrument.Multiplier - CommissionCurrency;
    }
    public bool IsDone() => Logic switch
    {
        TradeLogic.Open => Math.Abs(Position) == Volume,
        TradeLogic.Close => Position == 0,
        _ => throw new NotSupportedException($"Unknow TradeLogic: {Logic}!")
    };
    public bool IsClosed() => Logic switch {
        TradeLogic.Open => false,
        TradeLogic.Close => Position == 0,
        _ => throw new NotSupportedException($"Unknow TradeLogic: {Logic}!")
    };
    public void Start(IConnector connector)
    {
        if (Instrument.LastTradeDate < DateTime.Now) return;
        connector.RequestMarketData(Instrument);
        connector.ReqMarketRule(Instrument.MarketRuleId);
        updateTradeInfo();
    }
    public void Work(IConnector connector, ILogger<ContainerTrader> logger, string account, int priceShift)
    {
        updateTradeInfo();
        var tradablePrice = Logic switch {
            TradeLogic.Open => Instrument.TradablePrice(Direction),
            TradeLogic.Close => Instrument.TradablePrice(getCloseDirection()),
            _ => throw new NotSupportedException("Unknow trade logic!")
        };
        switch (Logic)
        {
            case TradeLogic.Open when OpenOrder == null:
                if (Math.Abs(Position) == Volume) break;
                if (tradablePrice <= 0m) break;
                createAndSendOrder(true, connector, account, priceShift);
                break;
            case TradeLogic.Open when OpenOrder != null:
                if (!connector.IsOrderOpen(OpenOrder))
                {
                    OpenOrder = null;
                    break;
                }
                if (StrategyHelper.OrderPriceOutBound(OpenOrder, tradablePrice, Instrument.MinTick))
                {
                    logger.LogError("Order out of bound!\n" +
                        "LimitPrice = {OpenOrder.LimitPrice}\n" +
                        "TradablePrice = {tadablePrice}", OpenOrder.LimitPrice, tradablePrice);
                    connector.CancelOrder(OpenOrder);
                }
                break;
            case TradeLogic.Close when OpenOrder == null:
                if (Position == 0) break;
                createAndSendOrder(false, connector, account);
                break;
            case TradeLogic.Close when OpenOrder != null:
                if (!connector.IsOrderOpen(OpenOrder))
                {
                    OpenOrder = null;
                    break;
                }
                
                if (StrategyHelper.OrderPriceOutBound(OpenOrder, Instrument.GetBidAskTradablePrice(getCloseDirection()), Instrument.MinTick))
                {
                    logger.LogError("Order out of bound!\n" +
                        "LimitPrice = {OpenOrder.LimitPrice}\n" +
                        "TradablePrice = {tadablePrice}", OpenOrder.LimitPrice, tradablePrice);
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
