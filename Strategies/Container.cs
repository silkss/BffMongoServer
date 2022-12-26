using Common.Types.Instruments;
using Connectors;
using MongoDB.Bson.Serialization.Attributes;
using Strategies.Settings;
using System.Collections.Generic;

namespace Strategies;

public class Container
{
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonIgnore]
    public bool InTrade { get; private set; }
    public Instrument Instrument { get; set; }
    public ContainerSettings ContainerSettings { get; set; }
    public List<OptionStrategy> OptionStrategies { get; } = new();
    public void Start(IConnector connector)
    {
        connector.RequestMarketData(Instrument);
        foreach (var optionStrategy in OptionStrategies)
            optionStrategy.Start(connector);
    }
}
