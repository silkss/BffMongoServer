namespace Strategies.Base;

using Common.Types.Instruments;
using Connectors;
using MongoDB.Bson.Serialization.Attributes;
using Strategies.Types;
using Strategies.Settings;
using System.Linq;
using Common.Types.Base;
using Strategies.BatmanStrategy;
using System.Threading;

public abstract class Container
{
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonIgnore]
    public bool InTrade { get; protected set; }
    public Instrument? Instrument { get; set; }
    public abstract void Start(IConnector connector);
    public abstract void Stop(IConnector connector);
    public abstract void Work(IConnector connector);
    public abstract void Close();
}
