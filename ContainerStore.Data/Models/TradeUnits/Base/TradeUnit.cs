using ContainerStore.Common.Enums;
using ContainerStore.Data.Models.Transactions;
using System;
using System.Collections.Generic;


namespace ContainerStore.Data.Models.TradeUnits.Base;

public class TradeUnit
{
    public Instrument? Instrument { get; set; }
    public int Volume { get; set; } = 1;
    public int Position { get; set; }
    public TradeLogic Logic { get; set; }
    public List<Transaction>? Transactions { get; } = new();
    public Directions Direction { get; set; }
    public int TradableQuantity() => Logic switch
    {
        TradeLogic.Open => Volume - Math.Abs(Position),
        TradeLogic.Close => Math.Abs(Position),
        _ => throw new ArgumentOutOfRangeException(nameof(Logic), $"Not expected direction value: {Logic}")
    };
    public bool IsDone() => Logic switch
    {
        TradeLogic.Open => Volume > Math.Abs(Position),
        TradeLogic.Close => Position == 0,
        _ => throw new ArgumentOutOfRangeException(nameof(Logic), $"Not expected direction value: {Logic}")
    };
    
}
