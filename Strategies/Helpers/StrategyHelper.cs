using Common.Types.Base;
using Common.Types.Orders;
using Strategies.Settings;
using System;
using System.Collections.Generic;

namespace Strategies.Helpers;

public static class StrategyHelper
{
    public static (int pos, decimal closedPnl, decimal commission) GetPosition(IEnumerable<Order> orders)
    {
        var position = 0;
        var pnl = 0m;
        var commission = 0m;
        foreach (var order in orders)
        {
            if (order.Direction == Directions.Buy)
            {
                position += order.FilledQuantity;
                pnl -= (order.FilledQuantity * order.AvgFilledPrice);
            }
            else
            {
                position -= order.FilledQuantity;
                pnl += (order.FilledQuantity * order.AvgFilledPrice);
            }
            commission += order.Commission;
        }
        return (position, pnl, commission);
    }
    /// <summary>
    /// Вернет Истина если позиция равна объему.
    /// </summary>
    /// <param name="orders"></param>
    /// <param name="volume"></param>
    /// <returns></returns>
    public static bool Opened(IEnumerable<Order> orders, int volume) => 
        Math.Abs(GetPosition(orders).pos) == volume;
    public static bool Closed(IEnumerable<Order> orders) => 
        GetPosition(orders).pos == 0;
    public static bool OrderPriceOutBound(Order order, decimal actualPrice, ContainerSettings settings) =>
        Math.Abs(order.LimitPrice - actualPrice) > 5;// settings.OrderPriceShift * 4;
}   
