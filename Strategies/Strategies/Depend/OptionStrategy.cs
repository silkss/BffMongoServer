using Notifier;
using Connectors;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using Strategies.Helpers;
using Strategies.Settings;
using Strategies.Strategies.Base;
using Common.Types.Base;
using Common.Types.Orders;
using Common.Types.Instruments;

namespace Strategies.Strategies.Depend;

public class OptionStrategy : TradableStrategy
{
    public Instrument Instrument { get; set; }
    private Order createOpenOrder(MainSettings settings, decimal orderPrice) => new Order(this, settings.Account)
    {
        Quantity = getTradableVolume(),
        LimitPrice = orderPrice == 0 ? Instrument.TradablePrice(Direction) : orderPrice,
        Direction = Direction,
    };
    private Order createCloseOrder(MainSettings settings) => new Order(this, settings.Account)
    {
        Quantity = Math.Abs(StrategyHelper.GetPosition(Orders).pos),
        LimitPrice = Instrument.TradablePrice(GetCloseDirection()),
        Direction = GetCloseDirection(),
    };
    private void createAndSendOrder(bool isOpen, IConnector connector, MainSettings settings, decimal orderPrice = 0)
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
        (var pos, _) = StrategyHelper.GetPosition(Orders);
        return Volume - Math.Abs(pos);
    }

    private readonly object _transactionLock = new();

    public List<Order> Orders { get; set; } = new();
    public decimal OpenPrice { get; set; }

    [BsonIgnore]
    public Order? OpenOrder { get; private set; }
    public int Volume { get; set; }
    public TradeLogic Logic { get; set; }
    public Directions Direction { get; private set; }
    public decimal GetPnl()
    {
        (var pos, var pnl) = StrategyHelper.GetPosition(Orders);
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
            connector.CancelOrder(OpenOrder);
        Logic = TradeLogic.Close;

        if (Closure != null)
            Closure.Close(connector);
    }

    public bool IsDone() => Logic switch
    {
        TradeLogic.Open => StrategyHelper.Opened(Orders, Volume),
        TradeLogic.Close => StrategyHelper.Closed(Orders),
        _ => throw new ArgumentException($"Неизвестная логика работы стратегии! {Logic}"),
    };
    public bool IsClosured() => Logic switch
    {
        TradeLogic.Open when Closure == null => IsDone(),
        TradeLogic.Open when Closure != null => IsDone() && Closure.IsClosured(),
        TradeLogic.Close when Closure == null => IsDone(),
        TradeLogic.Close when Closure != null => IsDone() && Closure.IsClosured(),
        _ => throw new ArgumentException("Неизвестное состояние для стратегии!")
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
        switch (Logic)
        {
            case TradeLogic.Open when OpenOrder == null:
                if (IsDone()) break;
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
            case TradeLogic.Close when OpenOrder != null:
                if (!connector.IsOrderOpen(OpenOrder))
                {
                    OpenOrder = null;
                    break;
                }
                if (StrategyHelper.OrderPriceOutBound(OpenOrder, Instrument.TradablePrice(Direction), mainSettings))
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
        ClosureSettings closureSettings,
        decimal orderPrice = 0m)
    {
        switch (Logic)
        {
            case TradeLogic.Open when OpenOrder == null:
                if (IsClosured()) break;
                if (IsDone() && Closure != null)
                {
                    if (OpenPrice != 0m)
                        Closure.WorkWithClosure(connector, notifier, mainSettings, closureSettings, OpenPrice);
                    break;
                }
                if (Instrument.TradablePrice(Direction) == 0) break;
                orderPrice = orderPrice * closureSettings.ClosurePriceGapProcent / 100;
                createAndSendOrder(true, connector, mainSettings, orderPrice);
                break;
            case TradeLogic.Open when OpenOrder != null:
                if (!connector.IsOrderOpen(OpenOrder))
                {
                    OpenOrder = null;
                    var msg = $"{this.Instrument.FullName} | {mainSettings.Account}. Cant find open order";
                    notifier.LogInformation(msg, toTelegram: false);
                    break;
                }
                if (orderPrice != 0) break;
                if (StrategyHelper.OrderPriceOutBound(OpenOrder, Instrument.TradablePrice(Direction), mainSettings))
                {
                    var msg = $"{this.Instrument.FullName} | {mainSettings.Account}. Order out of bound.";
                    notifier.LogInformation(msg, toTelegram: false);
                    connector.CancelOrder(OpenOrder);
                }
                break;
            case TradeLogic.Close when OpenOrder == null:
                if (IsClosured()) break;
                if (IsDone() && Closure != null)
                {
                    Closure.WorkWithClosure(connector, notifier, mainSettings, closureSettings);
                    break;
                }
                if (Instrument.TradablePrice(Direction) == 0) break;
                createAndSendOrder(false, connector, mainSettings);
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
    public int GetPosition() => StrategyHelper.GetPosition(Orders).pos;
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
        if (Closure != null)
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
                Closure.Logic = TradeLogic.Open;
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
            Logic = TradeLogic.Open,
            Volume = volume,
            Direction = direction,
        };
    public static OptionStrategy CreateClosure(
        Instrument instrument, int volume = 1,
        Directions direction = Directions.Sell) => new OptionStrategy
        {
            Instrument = instrument,
            Logic = TradeLogic.Close,
            Volume = volume,
            Direction = direction
        };


    #endregion
}
