using System.Collections.Generic;
using System.Linq;

namespace Connectors.Ib.Caches;

internal class OpenOrdersCache
{
    private readonly List<Common.Types.Orders.Order> _openOrders = new();
    private readonly object _ordersLock = new();

    public void Add(Common.Types.Orders.Order order)
    {
        lock (_ordersLock)
        {
            _openOrders.Add(order);
        }
        order.CreatedTime = System.DateTime.Now;
        order.Status = "Sent";
    }
    public Common.Types.Orders.Order? GetById(int id)
    {
        Common.Types.Orders.Order? order = null;
        lock (_ordersLock)
        {
            order = _openOrders.FirstOrDefault(o => o.BrokerId == id);
        }
        return order;
    }
    public bool Contains(Common.Types.Orders.Order order)
    {
        var isOpen = false;
        lock(_ordersLock)
        {
            isOpen = _openOrders.Any(o => o.BrokerId == order.BrokerId);
        }
        return isOpen;
    }
    public bool Remove(Common.Types.Orders.Order item)
    {
        var removed = false;
        lock (_ordersLock)
        {
            removed = _openOrders.Remove(item);
        }
        return removed;
    }
}
