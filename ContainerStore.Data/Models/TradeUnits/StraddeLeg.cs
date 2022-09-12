using ContainerStore.Data.Models.TradeUnits.Base;

namespace ContainerStore.Data.Models.TradeUnits;

public class StraddleLeg : TradeUnit
{
    public Closure? Closure;
    public decimal OpenPrice { get; set; }
    public override void OnOrderFilled(int orderId)
    {
        if (OpenOrder == null) return;
        if (OpenOrder.BrokerId != orderId) return;
        if (OpenOrder.Direction == Direction)
        {
            OpenPrice = OpenOrder.AvgFilledPrice;
        }
        OpenOrder = null;
    }
}