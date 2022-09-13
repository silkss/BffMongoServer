using ContainerStore.Common.Enums;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Diagnostics.Metrics;
using System.Diagnostics;
using ContainerStore.Common.Helpers;

namespace ContainerStore.Data.Models;

public class Instrument
{
    public int Id { get; set; }
    public InstrumentType Type { get; set; }
    public string? FullName { get; set; }
    public string? Symbol { get; set; }
    public string? TradeClass { get; set; }
    public string? Exchange { get; set; }
    public string? Currency { get; set; }
    public decimal MinTick { get; set; }
    public int MarketRuleID { get; set; }
    public DateTime LastTradeDate { get; set; }
    public decimal Strike { get; set; }
    public OptionType OptionType { get; set; }
    public int Multiplier { get; set; }
    [BsonIgnore]
    public decimal Ask { get; set; }
    [BsonIgnore]
    public decimal Bid { get; set; }
    [BsonIgnore]
    public decimal Last { get; set; }
    [BsonIgnore]
    public decimal TheorPrice { get; set; }
    public void OnPriceChange(object? sender, PriceChangedEventArgs args)
    {
        if (args.TickerId != Id) return;

        switch (args.Tick)
        {
            case (Tick.Ask):
                Ask = args.Price;
                break;
            case (Tick.Bid):
                Bid = args.Price;
                break;
            case (Tick.Last):
                Last = args.Price;
                break;
            case (Tick.TheorPrice):
                TheorPrice = Helper.RoundUp(args.Price, MinTick) ;
                break;
            default:
                break;

        }
    }
    public decimal TradablePrice(Directions direction) => direction switch
    {
        Directions.Sell => Type switch
        {
            InstrumentType.Future => Last,
            InstrumentType.Option => TheorPrice < Bid ? Bid : TheorPrice,
            _ => Last,
        },
        Directions.Buy => Type switch
        {
            InstrumentType.Future => Last,
            InstrumentType.Option => TheorPrice > Ask ?  Ask : TheorPrice,
            _ => Last,
        },
        _ => Last,
    };
}
