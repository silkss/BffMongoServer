using Common.Types.Instruments;
using Connectors;
using Strategies.Strategies.TradeUnions;
using System.Collections.Generic;

namespace Strategies;

public class Container
{
    public Instrument Instrument { get; set; }
    public List<OptionStrategy> OptionStrategies { get; } = new();
    public void Start(IConnector connector)
    {
        connector.RequestMarketData(Instrument);
        foreach (var optionStrategy in OptionStrategies)
            optionStrategy.Start(connector);
    }
}
