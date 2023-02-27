namespace Strategies.Base;

using Common.Types.Base;
using Common.Types.Orders;
using System;
using System.Collections.Generic;

public static class StrategyHelper
{
    public static (int pos, decimal closedPnl, decimal commission, decimal enterPriceWithCommission) GetPosition(IEnumerable<Order> orders, Directions strategyDirection, int multiplier)
    {
        var pnl = 0m;
        var position = 0;
        var commission = 0m;
        var enterPriceWithCommission = 0m;

        foreach (var order in orders)
        {
            if (order.Status == "Filled" && order.Direction == strategyDirection)
            {
                if (strategyDirection == Directions.Sell)
                {
                    enterPriceWithCommission = -(order.AvgFilledPrice * order.Quantity * multiplier - order.Commission);
                }
                else
                {
                    enterPriceWithCommission = order.AvgFilledPrice * order.Quantity * multiplier - order.Commission;
                }
            }
            if (order.Direction == Directions.Buy)
            {
                position += order.FilledQuantity;
                pnl -= order.FilledQuantity * order.AvgFilledPrice;
            }
            else
            {
                position -= order.FilledQuantity;
                pnl += order.FilledQuantity * order.AvgFilledPrice;
            }
            commission += order.Commission;
        }
        return (position, pnl, commission, enterPriceWithCommission);
    }
    public static bool OrderPriceOutBound(Order order, decimal actualPrice, decimal minTick) =>
        Math.Abs(order.LimitPrice - actualPrice) > (8 * minTick);// settings.OrderPriceShift * 4;
}
