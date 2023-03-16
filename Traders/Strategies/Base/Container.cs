namespace Traders.Strategies.Base;

using Common.Types.Instruments;
using Connectors;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization.Attributes;

public abstract class Container
{
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string? Id { get; set; }

    private bool _inTrade;

    [BsonIgnore]
    public bool InTrade
    {
        get => _inTrade;
        protected set => _inTrade = value;
    }

    public Instrument? Instrument { get; set; }
    public abstract void Start(IConnector connector, ILogger<ContainerTrader> logger);
    public abstract void Stop(IConnector connector, ILogger<ContainerTrader> logger);
    public abstract void Work(IConnector connector, ILogger<ContainerTrader> logger);
    public abstract void Close();
}
