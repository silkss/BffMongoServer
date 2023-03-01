namespace Traders.Strategies.Base;

using Common.Types.Instruments;
using Connectors;
using MongoDB.Bson.Serialization.Attributes;

public abstract class Container
{
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string? Id { get; set; }
    private bool inTrade;

    [BsonIgnore]
    public bool InTrade
    {
        get => inTrade;
        protected set => inTrade = value;
    }

    public Instrument? Instrument { get; set; }
    public abstract void Start(IConnector connector);
    public abstract void Stop(IConnector connector);
    public abstract void Work(IConnector connector);
    public abstract void Close();
}
