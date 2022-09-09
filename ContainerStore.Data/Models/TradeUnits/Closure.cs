using ContainerStore.Data.Models.TradeUnits.Base;

namespace ContainerStore.Data.Models.TradeUnits;

public class Closure : TradeUnit
{
    public override void OnOrderFilled(int brokerId)
    {
        throw new System.NotImplementedException();
    }
}
