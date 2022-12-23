using Connectors;
using Strategies.TradeUnits;
using System.Collections.Generic;

namespace Strategies;

public class OptionStrategy
{
    public List<OptionTradeUnit> OptionsTradeUnits { get; } = new();
    public void Start(IConnector connector)
    {
        foreach (var tradeUnit in OptionsTradeUnits)
        {
            tradeUnit.Start(connector);
        }
    }
}
