namespace Strategies;

using Common.Types.Base;
using Connectors;
using Strategies.Settings;
using Strategies.TradeUnits;
using System.Collections.Generic;

public class OptionStrategy
{
    public List<OptionTradeUnit> OptionsTradeUnits { get; set; } = new();
    public OptionStrategy? Closure { get; set; }
    public TradeLogic Logic { get; set; }
    public void Start(IConnector connector)
    {
        foreach (var tradeUnit in OptionsTradeUnits)
        {
            tradeUnit.Start(connector);
        }
        if (Closure != null)
            Closure.Start(connector);
    }
    public void AddTradeUnit(OptionTradeUnit unit)
    {
        lock (OptionsTradeUnits)
            OptionsTradeUnits.Add(unit);
    }
    public void Work(IConnector connector, ContainerSettings containerSettings)
    {
        lock (OptionsTradeUnits)
        {
            foreach (var unit in OptionsTradeUnits)
            {
                unit.Work(connector, containerSettings);
            }
        }
        if (IsDone() && Closure != null)
        {
            if (Logic == TradeLogic.Open  && Closure.Logic == TradeLogic.Close)
            {
                Closure.Logic = TradeLogic.Open;
            }
            if (Logic == TradeLogic.Close && Closure.Logic == TradeLogic.Open)
            {
                Closure.Close();
            }
            Closure.Work(connector, containerSettings);
        }
    }
    public bool IsDone()
    {
        lock (OptionsTradeUnits)
        {
            foreach (var unit in OptionsTradeUnits)
            {
                if (!unit.IsDone()) return false;
            }
        }

        return true;
    }
    public void Stop(IConnector connector)
    {
        lock (OptionsTradeUnits)
            OptionsTradeUnits.ForEach(otu => otu.Stop(connector));
    }
    public decimal GetCurrencyPnl()
    {
        var pnl = 0m;
        lock (OptionsTradeUnits)
        {
            foreach (var unit in OptionsTradeUnits)
            {
                pnl += unit.GetCurrencyPnl();
            }
        }
        return pnl;
    }
    public void Close()
    {
        lock (OptionsTradeUnits)
        {
            foreach (var unit in OptionsTradeUnits)
                unit.Close();
        }
    }
}
