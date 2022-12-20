using Common.Enums;

namespace GreatOptionTraderStrategies.Strategies.Base.Settings;

public class StraddleSettings
{
    public Directions BaseDirections { get; set; }
    public int MinDaysToExpiration { get; set; }
    public string? TradingOptionClass { get; set; }
}
