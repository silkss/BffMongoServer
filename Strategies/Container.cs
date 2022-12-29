using Common.Types.Instruments;
using Connectors;
using MongoDB.Bson.Serialization.Attributes;
using Strategies.Types;
using Strategies.Settings;
using System.Collections.Generic;
using System.Linq;
using Common.Types.Base;

namespace Strategies;

public class Container
{
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonIgnore]
    public bool InTrade { get; private set; }
    public Instrument? Instrument { get; set; }
    public ContainerSettings? ContainerSettings { get; set; }
    public OptionStrategySettings? OptionStrategySettings { get; set; }
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
}
