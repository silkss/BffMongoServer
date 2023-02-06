namespace Strategies.Containers;

using Common.Types.Base;
using Connectors;
using Strategies.Settings;
using Strategies.TradeUnits.Spreads;
using System.Collections.Generic;
using System.ComponentModel;

public class SpreadWithHedgeSpreadContainer : Base.BaseContainer
{
    public List<SpreadWithHedgeSpread> Spreads { get; set; } = new();
    public SpreadWithHedgeSpreadSettings? SpreadWithHedgeSpreadSettings { get; set; }

    public override bool IsWorking() 
    {
        foreach (var spread in Spreads)
        {
            if (spread.Logic == TradeLogic.Open)
                return true;
        }
        return false;
    }
    public override string CreateSpread(IConnector connector, double basisPrice, bool isLong)
    {
        if (Instrument == null) return "Parent instrument is NULL!";
        if (OptionStrategySettings == null) return "No OptionStrategy settings!";
        if (SpreadWithHedgeSpreadSettings == null) return "No SpreadSettings";

        var tradingClass = connector.GetOptionTradingClass(
            Instrument.Id,
            OptionStrategySettings.GetMinExpirationDate());

        if (tradingClass == null)
        {
            return "Cant find option class for request!";
        }

        var msg = SpreadWithHedgeSpread.Create(
            connector, tradingClass, Instrument,
            basisPrice,
            SpreadWithHedgeSpreadSettings,
            OptionStrategySettings,
            isLong, out var spread);

        if (spread == null)
            return msg;

        lock (Spreads)
        {
            Spreads.Add(spread);
        }
        return "Created";
    }
}
