﻿using System;
using System.Linq;
using System.Collections.Generic;
using ContainerStore.Connectors;
using Instruments;
using Strategies.Depend;
using Strategies.Enums;
using Strategies.Settings;
using Strategies.Settings.Straddle;
using TraderBot.Notifier;

namespace Strategies.TradeUnions;

public class Straddle
{
    private readonly TimeSpan _2days = new TimeSpan(days: 2, 0, 0, 0);
    private readonly TimeSpan _4days = new TimeSpan(days: 4, 0, 0, 0);

    private bool checkProfitLevels(List<ProfitLevel>? levels, int daysAfterOpen, Notifier notifier)
    {
        if (levels == null)
        {
            notifier.LogInformation("Уровни \"замкнутого\" профита не заданы!");
            return false;
        }
        else
        {
            var wantedProfit = levels
                .Where(level => level.MaxDaysAfterCreation <= daysAfterOpen)
                .MaxBy(level => level.MaxDaysAfterCreation)?
                .ProfitMinimum;

            if (wantedProfit == null)
            {
                return false;
            }
            return GetCurrencyPnl() > wantedProfit;
        }
    }
	public Straddle() { }
    public Straddle(Instrument call, Instrument put)
	{
        Legs.Add(OptionStrategy.CreateStraddleLeg(call, volume: 1));
		Legs.Add(OptionStrategy.CreateStraddleLeg(put, volume: 1));

        CreatedTime = DateTime.Now;
	}

	public Straddle(Instrument call, Instrument closureCall, Instrument put, Instrument closurePut)
    {
        var callLeg = OptionStrategy.CreateStraddleLeg(call);
        callLeg.Closure = OptionStrategy.CreateClosure(closureCall);
        Legs.Add(callLeg);

        var putLeg= OptionStrategy.CreateStraddleLeg(put);
        putLeg.Closure = OptionStrategy.CreateClosure(closurePut);
        Legs.Add(putLeg);

        CreatedTime = DateTime.Now;
    }
    public Logic Logic { get; private set; } = Logic.Open;
    public List<OptionStrategy> Legs { get; set; } = new List<OptionStrategy>(2);
	public DateTime CreatedTime { get; set; }
    public void Start(IConnector connector)
    {
        foreach (var leg in Legs)
        {
            leg.Start(connector);
        }
    }
    public bool CheckUnclosuredProfitLevels(StraddleSettings straddleSettings, Notifier notifier)
    {
        if (IsSomeLegIsClosured()) return false;
        var daysAfterCreation = (DateTime.Now - CreatedTime).Days;
        return checkProfitLevels(straddleSettings.UnClosuredProfitLevels, daysAfterCreation, notifier);
    }
    public bool CheckClosuredProfitLevels(StraddleSettings straddleSettings, Notifier notifier)
    {
        if (!IsSomeLegIsClosured()) return false;
        var daysAfterCreation = (DateTime.Now - CreatedTime).Days;
        return checkProfitLevels(straddleSettings.ClosuredProfitLevels, daysAfterCreation, notifier); ;
    }
    public void Work(IConnector connector, Notifier notifier, MainSettings settings, 
        ClosureSettings closureSettings)
    {
        foreach (var leg in Legs)
        {
            if (IsSomeLegIsClosured())
            {
                leg.Work(connector, settings);
            }
            else
            {
                leg.WorkWithClosure(connector, settings, closureSettings);
            }
        }
    }
    /// <summary>
    /// Вернет true если текущий ПиУ достиг необходимого уровня!
    /// </summary>
    /// <param name="settings"></param>
    /// <returns></returns>
    public bool CheckPnlForClose(StraddleSettings settings)
    {
        var pnl = GetCurrencyPnl();
        var now = DateTime.Now;
        if (now.DayOfWeek == DayOfWeek.Friday && CreatedTime.Date == now.Date)
        {
            return pnl > (settings.StraddleTargetPnl / 3);
        }
        var daysPassed = now - CreatedTime;
        if (daysPassed > _4days)
        {
            return pnl > (settings.StraddleTargetPnl / 4);
        }
        if (daysPassed > _2days)
        {
            return pnl > (settings.StraddleTargetPnl / 2);
        }
        return pnl > settings.StraddleTargetPnl;
    }
    public bool IsSomeLegIsClosured() => Legs.Any(leg => leg.IsClosured());
    public void Stop(IConnector connector)
    {
        foreach (var leg in Legs)
        {
            leg.Stop(connector);
        }
    }
    public void Close(IConnector connector)
    {
        Logic = Logic.Close;
        Legs.ForEach(l => l.Close(connector));
    }
    public DateTime GetCloseDate(int? days) => days is null 
        ? CreatedTime
        : CreatedTime.AddDays(days.Value);
    public decimal GetPnl() => Legs.Sum(leg => leg.GetPnlWithClosure());
    public decimal GetCurrencyPnl() => Legs.Sum(leg => leg.GetCurrencyPnlWithClosure());
    public bool IsDone() => Legs.All(leg => leg.IsClosured());
    public bool IsStartedWork() => Legs.Any(s => s.IsDone());
    public bool IsOpen() => Legs.All(leg => leg.Logic == Enums.Logic.Open);
}
