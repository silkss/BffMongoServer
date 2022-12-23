using Common.Types.Instruments;
using System;
using System.Collections.Generic;

namespace Common.Events;

public class PriceRuleEventArgs : EventArgs
{
    public int MarketRuleId { get; set; }
    public List<PriceBorder> PriceBorders { get; } = new();
}
