using System;
using System.Collections.Generic;
using System.Linq;
using ContainerStore.Connectors;
using ContainerStore.Connectors.Ib;
using Instruments;
using Strategies.Depend;
using Strategies.Settings;

namespace Strategies.TradeUnions;

public class Straddle
{
	public Straddle(Instrument call, Instrument put)
	{
		Legs[0] = OptionStrategy.CreateStraddleLeg(call, volume: 1);
		Legs[1] = OptionStrategy.CreateStraddleLeg(put, volume: 1);

        CreatedTime = DateTime.Now;
	}

	public Straddle(Instrument call, Instrument closureCall, Instrument put, Instrument closurePut)
    {
        Legs[0] = OptionStrategy.CreateStraddleLeg(call);
        Legs[0].Closure = OptionStrategy.CreateClosure(closureCall);

        Legs[1] = OptionStrategy.CreateStraddleLeg(put);
        Legs[1].Closure = OptionStrategy.CreateClosure(closurePut);

        CreatedTime = DateTime.Now;
    }

    public List<OptionStrategy> Legs { get; } = new List<OptionStrategy>(2);
	public DateTime CreatedTime { get; set; }
    public void Start(IConnector connector)
    {
        foreach (var leg in Legs)
        {
            leg.Start(connector);
        }
    }
    public void Work(IConnector connector, MainSettings settings)
    {
        foreach (var leg in Legs)
        {
            leg.Work(connector, settings);
        }
    }
    public void Stop(IConnector connector)
    {
        foreach (var leg in Legs)
        {
            leg.Stop(connector);
        }
    }
    public void Close(IConnector connector) => Legs.ForEach(l => l.Close(connector));
    public DateTime GetCloseDate(int? days) => days is null 
        ? CreatedTime
        : CreatedTime.AddDays(days.Value);
    public decimal GetPnl() => Legs.Sum(leg => leg.GetPnlWithClosure());
    public decimal GetCurrencyPnl() => Legs.Sum(leg => leg.GetCurrencyPnlWithClosure());
    public bool IsDone() => Legs.All(leg => leg.IsClosured());
    public bool IsStartedWork() => Legs.Any(s => s.IsDone());
    public bool IsOpen() => Legs.All(leg => leg.Logic == Enums.Logic.Open);
}
