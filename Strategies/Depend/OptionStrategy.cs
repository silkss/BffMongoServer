using ContainerStore.Common.Enums;
using ContainerStore.Connectors;
using Instruments;
using Strategies.Enums;
using Strategies.Helpers;
using Strategies.Settings;
using System;
using System.Collections.Generic;
using Transactions;

namespace Strategies.Depend;

public class OptionStrategy : Base.TradableStrategy
{

    private int getTradableVolume()
    {
        (var pos, _) = Strategy.GetPosition(Orders);
        return Volume - Math.Abs(pos);
    }

    private readonly object _transactionLock = new();

    public List<Transaction> Orders { get; } = new();
    public Transaction? OpenOrder { get; private set; }
    public int Volume { get; set; }
    public Logic Logic { get; set; }
    public Directions Direction { get; private set; }
    public decimal GetPnl()
    {
        (var pos, var pnl) = Strategy.GetPosition(Orders);
        pnl += Instrument.TradablePrice(GetCloseDirection()) * pos;
        return pnl;
    }
    public Directions GetCloseDirection() => Direction == Directions.Buy
        ? Directions.Sell
        : Directions.Buy;
    public OptionStrategy? Closure { get; set; }
    
    public decimal LimitPrice { get; set; }
    public void Close(IConnector connector)
    {
        if (OpenOrder != null)
        {
            connector.CancelOrder(OpenOrder);
        }
        Logic = Logic.Close;

        if (Closure!= null)
        {
            Closure.Close(connector);
        }
    }

    public bool IsDone() => Logic switch
    {
        Logic.Open => Strategy.Opened(Orders, Volume),
        Logic.Close => Strategy.Closed(Orders),
        _ => throw new System.ArgumentException("Неизвестная логика работы стратегии!", Logic.ToString()),
    };
    public bool IsClosured() => Logic switch
    {
        Logic.Open when Closure == null => IsDone(),
        Logic.Open when Closure != null => IsDone() && Closure.IsClosured(),
        Logic.Close when Closure == null => IsDone(),
        Logic.Close when Closure != null => IsDone() && Closure.IsClosured(),
        _ => throw new System.ArgumentException("Неизвестное состояние для стратегии!")
    };
    public void Start(IConnector connector)
    {
        connector.RequestMarketData(Instrument);
        connector.ReqMarketRule(Instrument.MarketRuleId);
        if (Closure != null)
        {
            Closure.Start(connector);
        }
    }
    public void Work(IConnector connector, MainSettings mainSettings, decimal orderPrice = 0m) 
    {
        switch (Logic)
        {
            case Logic.Open when OpenOrder == null:
                if (IsClosured()) break;
                if (IsDone() && Closure != null)
                {
                    Closure.Work(connector, mainSettings);
                    break;
                }
                if (Instrument.TradablePrice(Direction) == 0) break;
                OpenOrder = new Transaction(this, mainSettings.Account)
                {
                    Quantity = getTradableVolume(),
                    LimitPrice = orderPrice == 0 ? Instrument.TradablePrice(Direction) : orderPrice,
                    Direction = Direction,
                };
                connector.SendLimitOrder(Instrument, OpenOrder, mainSettings.OrderPriceShift, orderPrice == 0);
                break;

            case Logic.Close when OpenOrder == null:
                if (IsClosured()) break;
                if (IsDone() && Closure != null)
                {
                    Closure.Work(connector, mainSettings);
                    break;
                }
                if (Instrument.TradablePrice(Direction) == 0) break;
                OpenOrder = new Transaction(this, mainSettings.Account)
                {
                    Quantity = getTradableVolume(),
                    LimitPrice = Instrument.TradablePrice(GetCloseDirection()),
                    Direction = GetCloseDirection(),
                };
                connector.SendLimitOrder(Instrument, OpenOrder, mainSettings.OrderPriceShift, true);
                break;
            default:
                break;
        }
    }
    public void Stop(IConnector connector)
    {
        if (OpenOrder != null)
        {
            connector.CancelOrder(OpenOrder);
        }
        if (Closure != null)
        {
            Closure.Stop(connector);
        }
    }
    public decimal GetCurrencyPnl() => GetPnl() * Instrument.Multiplier;
    public decimal GetCurrencyPnlWithClosure()
    {
        var currency = GetCurrencyPnl();
        if (Closure != null)
        {
            currency += Closure.GetCurrencyPnlWithClosure();
        }
        return currency;
    }
    public decimal GetPnlWithClosure()
    {
        var pnl = GetPnl();
        if (Closure!=null)
        {
            pnl += Closure.GetPnlWithClosure();
        }
        return pnl;
    }

    #region IOrderHolder
    public override void OnOrderCancelled(int brokerId)
    {
        throw new System.NotImplementedException();
    }

    public override void OnOrderFilled(int brokerId)
    {
        throw new System.NotImplementedException();
    }

    public override void OnSubmitted(int brokerId)
    {
        throw new System.NotImplementedException();
    }
    #endregion

    #region Creating of depending strategies
    public static OptionStrategy CreateStraddleLeg(
        Instrument instrument, int volume = 1, 
        Directions direction = Directions.Buy) => new OptionStrategy
        {
            Instrument = instrument,
            Logic = Logic.Open,
            Volume = volume,
            Direction = direction,
        };
    public static OptionStrategy CreateClosure(
        Instrument instrument, int volume = 1,
        Directions direction = Directions.Sell) => new OptionStrategy
        {
            Instrument = instrument,
            Logic = Logic.Close,
            Volume = volume,
            Direction = direction
        };

   
    #endregion
}
