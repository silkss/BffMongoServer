using Common.Enums;
using Connectors;
using IBApi;
using Instruments;
using MongoDB.Bson.Serialization.Attributes;
using Strategies.Enums;
using Strategies.Helpers;
using Strategies.Settings;
using System;
using System.Collections.Generic;
using Notifier;
using Transactions;

namespace Strategies.Depend;

public class OptionStrategy : Base.TradableStrategy
{

    private Transaction createOpenOrder( MainSettings settings, decimal orderPrice) => new Transaction(this, settings.Account)
    {
        Quantity = getTradableVolume(),
        LimitPrice = orderPrice == 0 ? Instrument.TradablePrice(Direction) : orderPrice,
        Direction = Direction,
    };
    private Transaction createCloseOrder(MainSettings settings) => new Transaction(this, settings.Account)
    {
        Quantity = Math.Abs(Strategy.GetPosition(Orders).pos),
        LimitPrice = Instrument.TradablePrice(GetCloseDirection()),
        Direction = GetCloseDirection(),
    };
    private void createAndSendOrder(bool isOpen, IConnector connector, MainSettings settings, decimal orderPrice=0)
    {
        if (isOpen)
            OpenOrder = createOpenOrder(settings, orderPrice);
        else
            OpenOrder = createCloseOrder(settings);
        lock (_transactionLock)
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

    private int getTradableVolume()
    {
        (var pos, _) = Strategy.GetPosition(Orders);
        return Volume - Math.Abs(pos);
    }

    private readonly object _transactionLock = new();

    public List<Transaction> Orders { get; set; } = new();
    public decimal OpenPrice { get; set; }

    [BsonIgnore]
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
        if (Instrument.LastTradeDate < DateTime.Now) return;
        connector.RequestMarketData(Instrument);
        connector.ReqMarketRule(Instrument.MarketRuleId);
        if (Closure != null)
        {
            Closure.Start(connector);
        }
    }
    public void Work(IConnector connector, IBffLogger notifier, MainSettings mainSettings, decimal orderPrice = 0m) 
    {
        switch(Logic)
        {
            case Logic.Open when OpenOrder == null:
                if (IsDone()) break;
                createAndSendOrder(true, connector, mainSettings, orderPrice);
                break;
            case Logic.Open when OpenOrder != null:
                if (!connector.IsOrderOpen(OpenOrder))
                {
                    OpenOrder = null;
                    break;
                }
                if (Strategy.OrderPriceOutBound(OpenOrder, Instrument.TradablePrice(Direction), mainSettings))
                    connector.CancelOrder(OpenOrder);
                break;
            case Logic.Close when OpenOrder == null:
                if (IsDone())
                {
                    if (Closure != null)
                    {
                        Closure.Work(connector, notifier, mainSettings);
                    }
                    break;
                }
                createAndSendOrder(false, connector, mainSettings, orderPrice);
               
                break;
            case Logic.Close when OpenOrder != null:
                if (!connector.IsOrderOpen(OpenOrder))
                {
                    OpenOrder = null;
                    break;
                }
                if (Strategy.OrderPriceOutBound(OpenOrder, Instrument.TradablePrice(Direction), mainSettings))
                    connector.CancelOrder(OpenOrder);
                if (Closure != null)
                {
                    Closure.Work(connector, notifier, mainSettings);
                }
                break;
            default:
                break;
            
        }
    }
    public void WorkWithClosure(IConnector connector, IBffLogger notifier,
        MainSettings mainSettings, 
        ClosureSettings closureSettings, decimal orderPrice = 0m)
    {
        switch (Logic)
        {
            case Logic.Open when OpenOrder == null:
                if (IsClosured()) break;
                if (IsDone() && Closure != null)
                {
                    if (OpenPrice != 0m)
                        Closure.WorkWithClosure(connector, notifier, mainSettings, closureSettings, OpenPrice);
                    break;
                }
                if (Instrument.TradablePrice(Direction) == 0) break;
                orderPrice = (orderPrice * closureSettings.ClosurePriceGapProcent) / 100;
                createAndSendOrder(true, connector, mainSettings, orderPrice);
                break;
            case Logic.Open when OpenOrder != null:
                if (!connector.IsOrderOpen(OpenOrder))
                {
                    OpenOrder = null;
                    var msg = $"{this.Instrument.FullName} | {mainSettings.Account}. Cant find open order";
                    notifier.LogInformation(msg, toTelegram: false);
                    break;
                }
                if (orderPrice != 0) break;
                if (Strategy.OrderPriceOutBound(OpenOrder, Instrument.TradablePrice(Direction), mainSettings))
                {
                    var msg = $"{this.Instrument.FullName} | {mainSettings.Account}. Order out of bound.";
                    notifier.LogInformation(msg, toTelegram: false);
                    connector.CancelOrder(OpenOrder);
                }
                break;
            case Logic.Close when OpenOrder == null:
                if (IsClosured()) break;
                if (IsDone() && Closure != null)
                {
                    Closure.WorkWithClosure(connector, notifier, mainSettings, closureSettings);
                    break;
                }
                if (Instrument.TradablePrice(Direction) == 0) break;
                createAndSendOrder(false, connector, mainSettings);
                break;
            case Logic.Close when OpenOrder != null:
                if (!connector.IsOrderOpen(OpenOrder))
                {
                    OpenOrder = null;
                    break;
                }
                if (Strategy.OrderPriceOutBound(OpenOrder, Instrument.TradablePrice(Direction), mainSettings))
                    connector.CancelOrder(OpenOrder);
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
    public int GetPosition() => Strategy.GetPosition(Orders).pos;
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
        if (OpenOrder == null) return;
        if (OpenOrder.BrokerId != brokerId) return;
        OpenOrder = null;
    }

    public override void OnOrderFilled(int brokerId)
    {
        if (OpenOrder == null) return;
        if (OpenOrder.BrokerId != brokerId) return;
        if (OpenOrder.Direction == Direction)
        {
            OpenPrice = OpenOrder.AvgFilledPrice;
            if (Closure != null)
            {
                Closure.Logic = Logic.Open;
            }
        }
        OpenOrder = null;
    }

    public override void OnSubmitted(int brokerId) 
    { 
        if (OpenOrder == null) return;
        if (OpenOrder.BrokerId != brokerId) return;
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
