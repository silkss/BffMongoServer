using ContainerStore.Connectors;
using ContainerStore.Connectors.Ib;
using ContainerStore.Data.Models;

namespace ContainerStore.Traders.Helpers;

internal static class OrdersHelper
{
    public static void CancelStraddleOrders(IConnector connector, Straddle straddle)
    {
        connector
            .CancelOrder(straddle.CallLeg.OpenOrder)
            .CancelOrder(straddle.CallLeg.Closure?.OpenOrder)
            .CancelOrder(straddle.PutLeg.OpenOrder)
            .CancelOrder(straddle.PutLeg.Closure?.OpenOrder);
    }

    public static void CancelContainerOrders(IConnector connector, Container container)
    {
        foreach (var straddle in container.Straddles)
            CancelStraddleOrders(connector, straddle);
    }
        
}
