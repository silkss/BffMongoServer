using ContainerStore.Common.Enums;
using System;

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
    public DateOnly LastTradeDate { get; set; }
    public decimal Strike { get; set; }
    public OptionType OptionType { get; set; }
    public int Multiplier { get; set; }
}
