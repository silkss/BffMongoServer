using GreatOptionTraderStrategies.Strategies.Base.Settings;

namespace GreatOptionTraderStrategies.Strategies.Base;

public class Container : Strategy
{
    public ContainerSettings? MainSettings { get; set; }
    public StraddleSettings? StraddleSettings { get; set; }
}
