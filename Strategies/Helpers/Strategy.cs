using Common.Enums;
using Strategies.Settings;
using System;
using System.Collections.Generic;
using Transactions;

namespace Strategies.Helpers;

public static class Strategy
{
    public static (int pos, decimal closedPnl) GetPosition(IEnumerable<Transaction> orders)
    {
        var position = 0;
        var pnl = 0m;
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
        }
        return (position, pnl);
    }

    /// <summary>
    /// Вернет Истина если позиция равна объему.
    /// </summary>
    /// <param name="orders"></param>
    /// <param name="volume"></param>
    /// <returns></returns>
    public static bool Opened(IEnumerable<Transaction> orders, int volume) => 
        Math.Abs(GetPosition(orders).pos) == volume;
    public static bool Closed(IEnumerable<Transaction> orders) => 
        GetPosition(orders).pos == 0;
    public static bool OrderPriceOutBound(Transaction order, decimal actualPrice, MainSettings settings) =>
        Math.Abs(order.LimitPrice - actualPrice) > settings.OrderPriceShift * 4;
}   
