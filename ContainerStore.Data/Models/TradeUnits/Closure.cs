using ContainerStore.Common.Enums;
using ContainerStore.Data.Models.Instruments;
using ContainerStore.Data.Models.TradeUnits.Base;

namespace ContainerStore.Data.Models.TradeUnits;

public class Closure : TradeUnit
{
    public Closure() : base()
    {
    }
    public Closure(Instrument instrument) :base()
    {
        Instrument = instrument;
        Logic = TradeLogic.Close;
        Direction = Directions.Sell;
    }
    public decimal LimitPrice { get; set; } = 0;
    public override void OnOrderFilled(int brokerId)
    {
        if (OpenOrder == null) return;
        if (OpenOrder.BrokerId != brokerId) return;
        OpenOrder = null;
    }
}
