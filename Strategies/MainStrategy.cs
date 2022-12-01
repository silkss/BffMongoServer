using Connectors;
using Common.Enums;
using Notifier;
using MongoDB.Bson.Serialization.Attributes;
using Strategies.Enums;
using Strategies.Settings;
using Strategies.Settings.Straddle;
using Strategies.TradeUnions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Strategies;

public class MainStrategy : Base.Strategy
{
    private readonly object straddleLock = new();

    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string? Id { get; set; }

    public MainSettings? MainSettings { get; set; }
    public StraddleSettings? StraddleSettings { get; set; }
    public ClosureSettings? ClosureSettings { get; set; }
    public List<Straddle> Straddles { get; set; } = new();
    public Straddle? GetOpenStraddle() =>
        Straddles.FirstOrDefault(s => s.IsOpen());
    public void AddStraddle(Straddle straddle, IConnector connector)
    {
        lock (straddleLock)
        {
            Straddles.ForEach(s => s.Close(connector));
            Straddles.Add(straddle);
        }
    }
    public StraddleStatus GetOpenStraddleStatus(IBffLogger notifier)
    {
        if (GetOpenStraddle() is Straddle straddle)
        {

            if (StraddleSettings != null)
            {
                if (straddle.CheckUnclosuredProfitLevels(StraddleSettings, notifier))
                    return StraddleStatus.UnClosuredProfitLevelReached;

                if (straddle.CheckClosuredProfitLevels(StraddleSettings, notifier))
                    return StraddleStatus.ClosuredProfitLevelReached;   
            }

            if (straddle.GetPnl() >= StraddleSettings?.StraddleTargetPnl)
                return StraddleStatus.InProfit;

            if (straddle.GetCloseDate(StraddleSettings?.StraddleLiveDays) <= DateTime.Now)
                return StraddleStatus.Expired;

            if (straddle.IsStartedWork() is false)
                return StraddleStatus.NotOpen;

            return StraddleStatus.Working;
        }
        return StraddleStatus.NotExist;
    }
    public decimal GetAllPnl() => Straddles.Sum(s => s.GetPnl());
    public decimal? GetOpenPnlCurrency() => GetOpenStraddle()?.GetCurrencyPnl();
    public decimal GetAllPnlCurrency() => Straddles.Sum(s => s.GetCurrencyPnl());
    public DateTime? GetApproximateCloseDate() => GetOpenStraddle()?
        .GetCloseDate(StraddleSettings?.StraddleLiveDays);
    public void Start(IConnector connector)
    {
        connector.RequestMarketData(Instrument);
        foreach (var straddle in Straddles)
            straddle.Start(connector);
    }
    public void Work(IConnector connector, IBffLogger notifier)
    {
        lock (straddleLock)
        {
            foreach (var straddle in Straddles)
            {
                if (StraddleSettings == null || 
                    MainSettings == null || 
                    ClosureSettings == null)
                {
                    notifier.LogError("Некоторые настройки равны NULL работа страддла не возможно!");
                    break;
                }
                if (straddle.Logic == Logic.Open)
                {
                    if (straddle.CheckPnlForClose(StraddleSettings))
                    {
                        notifier.LogInformation($"Reached Pnl!\n" +
                            $"{Instrument.FullName} | {MainSettings.Account}\n" +
                            $"{straddle.GetCurrencyPnl()}", toTelegram: true);
                        straddle.Close(connector);
                        continue;
                    }
                }
                straddle.Work(connector, notifier, MainSettings, ClosureSettings);
            }
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
