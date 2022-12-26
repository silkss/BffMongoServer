using Notifier;
using Connectors;
using MongoDB.Bson.Serialization.Attributes;
using Strategies.Types;
using Strategies.Settings;
using Strategies.Settings.Straddle;
using Strategies.Strategies.TradeUnions;
using System;
using System.Linq;
using System.Collections.Generic;
using Common.Types.Base;
using Common.Types.Instruments;

namespace Strategies.Strategies;

public class MainStrategy 
{
    private readonly object straddleLock = new();
    public Instrument Instrument { get; set; }

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
    public OptionStrategyStatus GetOpenStraddleStatus(IBffLogger notifier)
    {
        if (GetOpenStraddle() is Straddle straddle)
        {
            if (StraddleSettings != null)
            {
                if (straddle.CheckUnclosuredProfitLevels(StraddleSettings, notifier))
                    return OptionStrategyStatus.UnClosuredProfitLevelReached;

                if (straddle.CheckClosuredProfitLevels(StraddleSettings, notifier))
                    return OptionStrategyStatus.ClosuredProfitLevelReached;
            }
            if (straddle.GetPnl() >= StraddleSettings?.StraddleTargetPnl)
                return OptionStrategyStatus.InProfit;

            if (straddle.GetCloseDate(StraddleSettings?.StraddleLiveDays) <= DateTime.Now)
                return OptionStrategyStatus.Expired;

            if (straddle.IsStartedWork() is false)
                return OptionStrategyStatus.NotOpen;

            return OptionStrategyStatus.Working;
        }
        return OptionStrategyStatus.NotExist;
    }
    public decimal GetAllPnl() => Straddles.Sum(s => s.GetPnl());
    public decimal? GetOpenPnlCurrency() => GetOpenStraddle()?.GetCurrencyPnl();
    public decimal GetAllPnlCurrency() => Straddles.Sum(s => s.GetCurrencyPnl());
    public DateTime? GetApproximateCloseDate() => GetOpenStraddle()?
        .GetCloseDate(StraddleSettings?.StraddleLiveDays);
    public decimal? GetCurrentTargetPnl() => GetOpenStraddle()?.GetCurrentTargetPnl(StraddleSettings);
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
                if (straddle.Logic == TradeLogic.Open)
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
