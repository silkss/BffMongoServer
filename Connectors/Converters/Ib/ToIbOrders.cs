using Common.Types.Base;

namespace Connectors.Converters.Ib;

internal static class ToIbOrders
{
    public static IBApi.Order ToIbOrder(this Common.Types.Orders.Order order, 
        bool market = false) => new IBApi.Order
    {
        TotalQuantity = order.Quantity,
        Account = order.Account,
        Tif = "GTC",
        LmtPrice = (double)order.LimitPrice,
        Action = order.Direction == Directions.Buy ? "BUY" : "SELL",
        OrderType = market ? "MKT" : "LMT"
    };
}
