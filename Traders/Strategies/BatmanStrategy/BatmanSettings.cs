namespace Traders.Strategies.BatmanStrategy;
using System;

public class BatmanSettings
{
    public string? Account { get; set; }
    public int Volume { get; set; }
    public int OrderPriceShift { get; set; }
    public int MinDaysToExpiration { get; set; }

    public int BaseBuyStrikeShift { get; set; }
    public int BaseSellStrikeShift { get; set; }
    public int ClosureBuyStikeShift { get; set; }
    public int ClosureSellStikeShift { get; set; }

    public DateTime GetMinExpirationDate() => DateTime.Now.AddDays(MinDaysToExpiration);
}
