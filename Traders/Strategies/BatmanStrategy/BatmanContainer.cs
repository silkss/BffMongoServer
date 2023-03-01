namespace Traders.Strategies.BatmanStrategy;

using Connectors;
using System.Collections.Generic;

public class BatmanContainer : Base.Container
{
    public List<BatmanOptionStrategy> Strategies { get; set; } = new();
    public BatmanSettings? Settings { get; set; }
    public void AddStrategy(BatmanOptionStrategy newStrategy)
    {
        lock (Strategies)
        {
            Strategies.Add(newStrategy);
        }
    }
    public override void Close()
    {
        throw new System.NotImplementedException();
    }

    public override void Start(IConnector connector)
    {
        if (Instrument == null) return;
        connector.RequestMarketData(Instrument);
        connector.RequestOptionChain(Instrument);
        lock (Strategies)
        {
            foreach (var strategy in Strategies)
            {
                strategy.Start(connector);
            }
        }
        InTrade = true;
    }

    public override void Stop(IConnector connector)
    {
        lock (Strategies)
        {
            foreach (var strat in Strategies)
            {
                strat.Stop(connector);
            }
        }
        InTrade = false;
    }

    public override void Work(IConnector connector)
    {
        if (!InTrade) return;
        if (Settings == null) return;
        if (Instrument == null) return;


        lock (Strategies)
        {
            foreach (var strategy in Strategies)
            {
                strategy.Work(connector, Settings, Instrument.Last);
            }
        }
    }

    public decimal GetTotalCurrencyPnlWithCommission()
    {
        decimal pnl = 0m;
        foreach (var strategy in Strategies)
        {
            pnl += strategy.GetTotalCurrencyPnlWithCommission();
        }
        return pnl;
    }
}
