namespace Strategies;

using Common.Types.Base;
using Connectors;
using Strategies.Settings;
using Strategies.TradeUnits;
using System.Collections.Generic;

public class OptionStrategy
{
    public List<OptionTradeUnit> OptionsTradeUnits { get; set; } = new();
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
    public void Stop(IConnector connector)
    {
        lock (OptionsTradeUnits)
            OptionsTradeUnits.ForEach(otu => otu.Stop(connector));
    }
    public decimal GetPnl() => 12.0m;
}
