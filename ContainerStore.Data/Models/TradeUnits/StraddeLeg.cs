using ContainerStore.Data.Models.TradeUnits.Base;
using ContainerStore.Data.Models.Transactions;

namespace ContainerStore.Data.Models.TradeUnits;

public class StraddleLeg : TradeUnit
{
    public Closure? Closure;
    public decimal OpenPrice { get; set; }
    public Transaction? OpenOrder { get; set; }
}