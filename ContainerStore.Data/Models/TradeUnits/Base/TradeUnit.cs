using ContainerStore.Data.Models.Transactions;
using System.Collections.Generic;


namespace ContainerStore.Data.Models.TradeUnits.Base;

public class TradeUnit
{
    public Instrument? Instrument { get; set; }
    public List<Transaction>? Transactions { get; } = new();
}
