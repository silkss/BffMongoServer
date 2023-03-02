namespace Traders.Strategies.BatmanStrategy;

using Amazon.Runtime.Internal.Util;
using Connectors;
using Microsoft.Extensions.Logging;
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

    public override void Start(IConnector connector, ILogger<ContainerTrader> logger)
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
        logger.LogInformation("{this} is started", this);
    }

    public override void Stop(IConnector connector, ILogger<ContainerTrader> logger)
    {
        lock (Strategies)
        {
            foreach (var strat in Strategies)
            {
                strat.Stop(connector);
            }
        }
        
        InTrade = false;
        logger.LogInformation("{this} is stopped", this);
    }

    public override void Work(IConnector connector, ILogger<ContainerTrader> logger)
    {
        if (!InTrade) return;
        if (Settings == null) return;
        if (Instrument == null) return;

        lock (Strategies)
        {
            foreach (var strategy in Strategies)
            {
                strategy.Work(connector, logger, Settings, Instrument.Last);
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

    public override string ToString() =>
        $"Container-{Instrument?.FullName}-{Settings?.Account}";
}
