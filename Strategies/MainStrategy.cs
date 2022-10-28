using ContainerStore.Connectors;
using MongoDB.Bson.Serialization.Attributes;
using Strategies.Enums;
using Strategies.Settings;
using Strategies.TradeUnions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Strategies;

public class MainStrategy : Base.Strategy
{
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string? Id { get; set; }

    public MainSettings? MainSettings { get; set; }
    public StraddleSettings? StraddleSettings { get; set; }
    public ClosureSettings? ClosureSettings { get; set; }
    public List<Straddle> Straddles { get; set; } = new();

    public Straddle? GetOpenStraddle() => Straddles.FirstOrDefault(s => s.IsOpen());

    public void AddStraddle(Straddle straddle, IConnector connector)
    {
        Straddles.ForEach(s => s.Close(connector));
        Straddles.Add(straddle);
    }
    public StraddleStatus GetOpenStraddleStatus()
    {
        if (GetOpenStraddle() is Straddle straddle)
        {

            if (straddle.GetPnl() >= StraddleSettings?.StraddleTargetPnl)
                return StraddleStatus.InProfit;

            if (straddle.GetCloseDate(StraddleSettings?.StraddleLiveDays) >= DateTime.Now)
                return StraddleStatus.Expired;

            if (straddle.IsStartedWork() is false)
                return StraddleStatus.NotOpen;

            return StraddleStatus.Working;
        }
        return StraddleStatus.NotExist;
    }
    public DateTime? GetApproximateCloseDate() => GetOpenStraddle()?
        .GetCloseDate(StraddleSettings?.StraddleLiveDays);
    public void Start(IConnector connector)
    {
        connector.RequestMarketData(Instrument);
        foreach (var straddle in Straddles)
            straddle.Start(connector);
    }
    public void Work(IConnector connector)
    {
        foreach (var straddle in Straddles)
        {
            if (MainSettings == null) break;
            straddle.Work(connector, MainSettings);
        }
    }
    public void Stop(IConnector connector)
    {
        foreach (var straddle in Straddles)
        {
            straddle.Stop(connector);
        }
    }
    public DateTime GetApproximateExpirationDate() => StraddleSettings is null
        ? DateTime.Now.AddDays(30)
        : DateTime.Now.AddDays(StraddleSettings.StraddleExpirationDays);
            
        
}
