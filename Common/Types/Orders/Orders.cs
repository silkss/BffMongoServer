using Common.Types.Base;
using Common.Types.Orders.Asbstractions;
using System;

namespace Common.Types.Orders;

public class Order
{
    private IOrderHolder? _orderHolder;

    public Order(IOrderHolder holder, string? account)
    {
        _orderHolder = holder;
        Status = "Created";
        Account = account;
    }
    public int BrokerId { get; set; }
    public DateTime CreatedTime { get; set; } 
    public DateTime FilledTime { get; set; }
    public Directions Direction { get; set; }
    public string Status { get; set; }
    public string? Account { get; init; }
    public int Quantity { get; set; }
    public int FilledQuantity { get; set; }
    public decimal LimitPrice { get; set; }
    public decimal AvgFilledPrice { get; set; }
    public decimal Commission { get; set; }
    public void Filled()
    {
        FilledTime = DateTime.Now;
        Status = "Filled";
        if (_orderHolder == null) return;
        _orderHolder.OnOrderFilled(BrokerId);
    }
    public void Canceled()
    {
        Status = "Canceled";
        FilledTime = DateTime.Now;
        if (_orderHolder == null) return;

        _orderHolder.OnOrderCancelled(BrokerId);
    }
    public void Submitted()
    {
        Status = "Submitted";
        if (_orderHolder == null) return;
        _orderHolder.OnSubmitted(BrokerId);
    }
}
