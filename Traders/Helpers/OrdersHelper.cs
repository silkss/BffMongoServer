using Connectors;
using Strategies;
using Strategies.TradeUnions;

namespace Traders.Helpers;

internal static class OrdersHelper
{
    public static void CancelStraddleOrders(IConnector connector, Straddle straddle)
    {
        //connector
        //    .CancelOrder(straddle.CallLeg.OpenOrder)
        //    .CancelOrder(straddle.CallLeg.Closure?.OpenOrder)
        //    .CancelOrder(straddle.PutLeg.OpenOrder)
        //    .CancelOrder(straddle.PutLeg.Closure?.OpenOrder);
    }

    public static void CancelContainerOrders(IConnector connector, MainStrategy container)
    {
        foreach (var straddle in container.Straddles)
            CancelStraddleOrders(connector, straddle);
    }
        
}
