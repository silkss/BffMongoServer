using ContainerStore.Data.Models.Instruments.PriceRules;
using System;
using System.Collections.Generic;

namespace ContainerStore.Data.Models.Events;

public class PriceRuleEventArgs : EventArgs
{
    public int MarketRuleId { get; set; }
    public List<PriceBorder> PriceBorders { get; } = new();
}
