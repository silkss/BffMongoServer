namespace Strategies;

using Connectors;
using Strategies.Types;
using Strategies.Settings;
using Strategies.Containers.Base;
using System.Collections.Generic;
using System.Linq;
using Common.Types.Base;

public class Container :  BaseContainer
{
    public SpreadSettings? SpreadSettings { get; set; }
    public List<OptionStrategy> OptionStrategies { get; set; } = new();
    public OptionStrategy? OpenStrategy
    {
        get
        {
            OptionStrategy? open = null;
            lock (OptionStrategies)
            {
                open = OptionStrategies.FirstOrDefault(os => os.Logic == TradeLogic.Open);
            }
            return open;
        }
    }

    public void Start(IConnector connector)
    {
        if (Instrument == null) return;
        connector.RequestMarketData(Instrument);
        connector.RequestOptionChain(Instrument);
        foreach (var optionStrategy in OptionStrategies)
            optionStrategy.Start(connector);

        InTrade = true;
    }
    public void Stop(IConnector connector)
    {
        InTrade = false;
        lock (OptionStrategies)
            OptionStrategies.ForEach(os => os.Stop(connector));
    }
    public void Work(IConnector connector) 
    {
        if (!InTrade) return;
        lock (OptionStrategies)
            foreach (var os in OptionStrategies)
                os.Work(connector, ContainerSettings);
    }
    public void AddOptionStrategy(OptionStrategy strategy)
    {
        lock (OptionStrategies)
            OptionStrategies.Add(strategy);
    }

    public OptionStrategyStatus GetOpenStrategyStatus()
    {
        var open = OpenStrategy;
        if (open == null)
            return OptionStrategyStatus.NotExist;

        return OptionStrategyStatus.Working;
    }

    public override bool IsWorking() => throw new System.NotImplementedException();
    public override string CreateSpread(IConnector connector, double basisPrice, bool isLong) => 
        throw new System.NotImplementedException();
}
