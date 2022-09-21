using ContainerStore.Data.Models.Instruments;
using ContainerStore.Data.Models.TradeUnits.Base;
using MongoDB.Bson.Serialization.Attributes;

namespace ContainerStore.Data.Models.TradeUnits;

public class StraddleLeg : TradeUnit
{
    
    public Closure Closure { get; set; }
    public decimal OpenPrice { get; set; }
    public override void OnOrderFilled(int orderId)
    {
        if (OpenOrder == null) return;
        if (OpenOrder.BrokerId != orderId) return;
        if (OpenOrder.Direction == Direction)
        {
            OpenPrice = OpenOrder.AvgFilledPrice;
            if (Closure != null)
            {
                Closure.Logic = Common.Enums.TradeLogic.Open;
            }
        }
        OpenOrder = null;
    }
    public StraddleLeg AddClosure(Instrument instrument)
    {
        Closure = new Closure(instrument);
        return this;
    }
    public override decimal GetPnl() => base.GetPnl() + (Closure?.GetPnl() ?? 0);
    [BsonIgnore]
    public override decimal CurrencyPnl => base.CurrencyPnl + (Closure?.CurrencyPnl ?? 0);
}