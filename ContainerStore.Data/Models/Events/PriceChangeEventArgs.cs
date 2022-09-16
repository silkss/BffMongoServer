using System;
using ContainerStore.Common.Enums;

namespace ContainerStore.Data.Models.Events;

public class PriceChangedEventArgs : EventArgs
{
    public int TickerId { get; set; }
    public Tick Tick { get; set; }
    public decimal Price { get; set; }
}