using ContainerStore.Common.Enums;
using ContainerStore.Data.Models.TradeUnits;

namespace ContainerStore.Traders.Helpers;

internal static class TraderHelper
{
    public static void StraddleLegWork(StraddleLeg leg)
    {
        switch (leg.Logic)
        {
            case TradeLogic.Open when leg.OpenOrder == null:
                break;
            case TradeLogic.Close:
                break;
            default: break;
        }
    }
}
