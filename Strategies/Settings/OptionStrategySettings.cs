using System;

namespace Strategies.Settings;

public class OptionStrategySettings
{
    public decimal StrategyTargetPnl { get; set; }
    public int MinDaysToExpiration { get; set; }
    public int Volume { get; set; }
    public DateTime GetMinExpirationDate() => DateTime.Now.AddDays(MinDaysToExpiration);
}
