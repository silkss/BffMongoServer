using Common.Types.Base;
using Connectors;
using Strategies.Settings;
using Strategies.TradeUnits;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Strategies;

public class OptionStrategy
{
    public List<OptionTradeUnit> OptionsTradeUnits { get; } = new();
    public TradeLogic Logic { get; set; }
    public void Start(IConnector connector)
    {
        foreach (var tradeUnit in OptionsTradeUnits)
        {
            tradeUnit.Start(connector);
        }
    }
    public void AddTradeUnit(OptionTradeUnit unit)
    {
        lock (OptionsTradeUnits)
            OptionsTradeUnits.Add(unit);
    }
    public void Work(IConnector connector, ContainerSettings containerSettings)
    {
        lock (OptionsTradeUnits)
            foreach (var otu in OptionsTradeUnits)
                otu.Work(connector, containerSettings);
    }
}
