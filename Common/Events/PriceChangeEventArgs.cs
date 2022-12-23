using System;
using Common.Types.Base;

namespace Common.Events;

public class PriceChangedEventArgs : EventArgs
{
    public int TickerId { get; set; }
    public Tick Tick { get; set; }
    public decimal Price { get; set; }
}