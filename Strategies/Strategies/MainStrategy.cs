using MongoDB.Bson.Serialization.Attributes;
using Strategies.Settings;

namespace Strategies.Strategies;

public class MainStrategy : Base.Strategy
{
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string? Id { get; set; }

    public MainSettings? MainSettings { get; set; }
    public StraddleSettings? StraddleSettings { get; set; }
    public ClosureSettings? ClosureSettings { get; set; } 
}
