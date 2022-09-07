using ContainerStore.Data.Models.TradeUnits.Base;

namespace ContainerStore.Data.Models.TradeUnits;

public class StraddleLeg : TradeUnit
{
    public Closure? Closure;
    public decimal OpenPrice { get; set; }
}