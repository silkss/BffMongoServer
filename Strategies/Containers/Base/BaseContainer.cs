namespace Strategies.Containers.Base;

using Common.Types.Instruments;
using Connectors;
using MongoDB.Bson.Serialization.Attributes;
using Strategies.Settings;

public abstract class BaseContainer
{
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonIgnore]
    public bool InTrade { get; protected set; }
    public ContainerSettings? ContainerSettings { get; set; }
    public OptionStrategySettings? OptionStrategySettings { get; set; }
    public Instrument? Instrument { get; set; }
    public abstract bool IsWorking();
    public abstract string CreateSpread(IConnector connector, double basisPrice, bool isLong);
}
