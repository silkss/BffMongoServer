using ContainerStore.Common.Enums;
using ContainerStore.Data.Models.Transactions;
using IBApi;

namespace ContainerStore.Connectors.Converters.Ib;

internal static class ToIbOrders
{
    public static Order ToIbOrder(this Transaction order, bool market = false) => new Order
    {
        TotalQuantity = order.Quantity,
        Account = order.Account,
        Tif = "GTC",
        LmtPrice = (double)order.LimitPrice,
        Action = order.Direction == Directions.Buy ? "BUY" : "SELL",
        OrderType = market ? "MKT" : "LMT"
    };
}
