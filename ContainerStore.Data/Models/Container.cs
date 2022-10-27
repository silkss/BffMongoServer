using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using ContainerStore.Common.Enums;
using ContainerStore.Data.Models;
using Instruments;
using MongoDB.Bson.Serialization.Attributes;
using Strategies.Enums;

namespace ContainerStore.OLD.Data.Models;

public class Container 
{
    private readonly object _lock = new();

    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string? Id { get; set; }

    private Instrument _parentInstrument;
    public Instrument ParentInstrument
    {
        get => _parentInstrument;
        set =>  _parentInstrument = value;
    }


    public decimal TotalPnl { get; set; }

    [BsonIgnore]
    [JsonIgnore]
    public Straddle? OpenStraddle => Straddles.FirstOrDefault(s => s.Logic == TradeLogic.Open);

    [JsonIgnore]
    [BsonIgnore]
    public DateTime? ApproximateCloseDate => OpenStraddle?.CreatedTime.AddDays(StraddleLiveDays);
    
    [BsonIgnore]
    public decimal CurrencyPnl => Straddles.Sum(s => s.CurrencyPnl);
    [BsonIgnore]
    public decimal CurrencyOpenPnl => Straddles
        .FirstOrDefault(s => s.Logic == Logic.Open) is Straddle straddle
            ? straddle.CurrencyPnl
            : 0m;
    public List<Straddle> Straddles { get; private set; } = new();
    public decimal Pnl => Straddles.Sum(s => s.GetPnl());

    public void AddStraddle(Straddle straddle)
    {
        lock (_lock)
        {
            if (Straddles == null) Straddles = new();
            Straddles.Add(straddle);
        }
    }
    public void Close()
    {
        foreach (var straddle in Straddles)
        {
            straddle.Close();
        }
    }

    /// <summary>
    ///  Дата, больше которой должен быть экспирация опциона при добавлении стрэддла.
    /// </summary>
    /// <returns></returns>
    public DateTime GetApproximateExpirationDate() => DateTime.Now.AddDays(StraddleExpirationDays);
    public void Stop()
    {
        foreach (var straddle in Straddles)
        {
            straddle.Stop();
        }
    }

    public StraddleStatus StatusOfOpenStraddle()
    {
        if (OpenStraddle is null)
            return StraddleStatus.NotExist;

        if (CurrencyOpenPnl >= StraddleTargetPnl)
            return StraddleStatus.InProfit;

        if (OpenStraddle.CreatedTime > ApproximateCloseDate)
            return StraddleStatus.Expired;

        if (OpenStraddle.IsDone() is false)
            return StraddleStatus.NotOpen;

        return StraddleStatus.Working;
    }
}
