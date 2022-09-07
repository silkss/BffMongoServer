using System;

namespace ContainerStore.Data.Models.Transactions;

public class Transaction
{
    public int BrokerId { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Status { get; set; }
}
