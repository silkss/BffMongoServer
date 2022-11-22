using System.Collections.Generic;
using System.Linq;
using Transactions;

namespace Connectors.Ib.Caches;

internal class OpenOrdersCache
{
    private readonly List<Transaction> _openOrders = new();
    private readonly object _ordersLock = new();

    public void Add(Transaction order)
    {
        lock (_ordersLock)
        {
            _openOrders.Add(order);
        }
        order.CreatedTime = System.DateTime.Now;
        order.Status = "Sent";
    }
    public Transaction? GetById(int id)
    {
        Transaction? order = null;
        lock (_ordersLock)
        {
            order = _openOrders.FirstOrDefault(o => o.BrokerId == id);
        }
        return order;
    }
    public bool Contains(Transaction order)
    {
        var isOpen = false;
        lock(_ordersLock)
        {
            isOpen = _openOrders.Any(o => o.BrokerId == order.BrokerId);
        }
        return isOpen;
    }
    public bool Remove(Transaction item)
    {
        var removed = false;
        lock (_ordersLock)
        {
            removed = _openOrders.Remove(item);
        }
        return removed;
    }
}
