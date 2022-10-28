﻿using ContainerStore.Common;
using ContainerStore.Common.Enums;
using ContainerStore.Data.Models.Instruments;
using ContainerStore.Data.Models.Transactions;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using Transactions;

namespace ContainerStore.Data.Models.TradeUnits.Base;

public abstract class TradeUnit : IOrderHolder
{
    private readonly object _transactionLock = new();
    public TradeUnit()
    {
        _transactionLock = new object();
    }
    public TradeLogic Logic { get; set; }
    public Instrument Instrument { get; set; }
    public int Volume { get; set; } = 1;
    public int Position
    {
        get
        {
            int position = 0;
            if (Transactions == null) return position;
            lock (_transactionLock)
            {
                foreach (var order in Transactions)
                {
                    if (order.Direction == Directions.Buy)
                        position += order.FilledQuantity;
                    else
                        position -= order.FilledQuantity;
                }
            }
            return position;
        }
    }
    [BsonIgnore]
    public decimal CurrencyPnl => GetPnl() * Instrument.Multiplier;
    public decimal GetPnl() 
    {
        var pos = Position;
        decimal pnl = 0m;
        if (Transactions == null) return pnl;

        lock (_transactionLock)
        {
            foreach (var trasaction in Transactions)
            {
                if (trasaction.Direction == Directions.Buy)
                {
                    pnl -= (trasaction.FilledQuantity * trasaction.AvgFilledPrice);
                }
                else
                {
                    pnl += (trasaction.FilledQuantity * trasaction.AvgFilledPrice);
                }
            }
        }
        if (Instrument != null)
        {
            pnl += Instrument.TradablePrice(CloseDirection()) * pos;
        }
        return pnl;
    }

    [BsonIgnore]
    public Transaction? OpenOrder { get; set; }
    public List<Transaction>? Transactions { get; set; }
    public Directions Direction { get; set; }
    public Directions CurrentDirection() => Logic switch 
    {
        TradeLogic.Open => Direction,
        TradeLogic.Close => CloseDirection(),
        _ => Direction
    };

    public Directions CloseDirection() => Direction == Directions.Buy ? Directions.Sell : Directions.Buy;
    public int TradableQuantity() => Logic switch
    {
        TradeLogic.Open => Volume - Math.Abs(Position),
        TradeLogic.Close => Math.Abs(Position),
        _ => throw new ArgumentOutOfRangeException(nameof(Logic), $"Not expected direction value: {Logic}")
    };
    public bool IsDone() => Logic switch
    {
        TradeLogic.Open => Volume == Math.Abs(Position),
        TradeLogic.Close => Position == 0,
        _ => throw new ArgumentOutOfRangeException(nameof(Logic), $"Not expected direction value: {Logic}")
    };
    public Transaction CreateClosingOrder(string account) => CreateOrder(account, CloseDirection());
    public Transaction CreateOpeningOrder(string account) => CreateOrder(account, Direction);
    public Transaction CreateOrder(string account, Directions direction)
    {
        OpenOrder = new Transaction(this)
        {
            Direction = direction,
            Account = account,
            Quantity = TradableQuantity(),
        };
        if (Transactions == null)
        {
            Transactions = new();
        }
        lock (_transactionLock)
        {
            Transactions.Add(OpenOrder);
        }
        return OpenOrder;
    }
    public abstract void OnOrderFilled(int brokerId);
    public void OnOrderCancelled(int brokerId)
    {
        if (OpenOrder == null) return;
        if (OpenOrder.BrokerId != brokerId) return;
        OpenOrder = null;
    }
    public void OnSubmitted(int brokerId)
    {
        if (OpenOrder == null) return;
        if (OpenOrder.BrokerId != brokerId) return;
    }
    public void Stop()
    {
        OpenOrder = null;
    }
}
