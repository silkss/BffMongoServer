using Common.Enums;
using Common.Helpers;
using Instruments.Events;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Instruments;

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
    public DateTime LastTradeDate { get; set; }
    public decimal Strike { get; set; }
    public OptionType OptionType { get; set; }
    public int MarketRuleId { get; set; }
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
            case Tick.Ask:
                Ask = args.Price;
                break;
            case Tick.Bid:
                Bid = args.Price;
                break;
            case Tick.Last:
                Last = args.Price;
                break;
            case Tick.TheorPrice:
                TheorPrice = args.Price;
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
            InstrumentType.Option => TheorPrice < Bid ? Bid : Helper.RoundUp(TheorPrice, MinTick),
            _ => Last,
        },
        Directions.Buy => Type switch
        {
            InstrumentType.Future => Last,
            InstrumentType.Option => TheorPrice > Ask && Ask > 0 ? Ask : Helper.RoundUp(TheorPrice, MinTick),
            _ => Last,
        },
        _ => Last,
    };
}
