using Instruments.PriceRules;
using System;
using System.Collections.Generic;

namespace Instruments.Events;

public class PriceRuleEventArgs : EventArgs
{
    public int MarketRuleId { get; set; }
    public List<PriceBorder> PriceBorders { get; } = new();
}
