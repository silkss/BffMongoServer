using ContainerStore.Common.Enums;
using System;

namespace ContainerStore.Data.Models.Transactions;

public class Transaction
{
    public int BrokerId { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Status { get; set; }
    public Directions Direction { get; set; }
    public string Account { get; set; }
    public int Quantity { get; set; }
    public decimal LimitPrice { get; set; }
}
