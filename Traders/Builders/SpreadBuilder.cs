namespace Traders.Builders;

using System;
using System.Linq;
using Common.Types.Base;
using Common.Types.Instruments;
using Connectors;
using Strategies;

public static class SpreadBuilder
{
    private static string createSpread(Instrument buy, Instrument sell, Container container, IConnector connector)
    {
        var spread = new OptionStrategy() { Logic = TradeLogic.Open };
        spread.AddTradeUnit(new(buy, Directions.Buy, container.OptionStrategySettings!.Volume));
        spread.AddTradeUnit(new(sell, Directions.Sell, container.OptionStrategySettings!.Volume));
        spread.Start(connector);

        container.AddOptionStrategy(spread);

        return $"Spread added {spread}";
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="price"></param>
    /// <param name="container">Тут должны хранится настройки, 
    /// согласно которым будут выбираться опционные инструменты.</param>
    /// <param name="oc">Опционный торговый класс, внутри которого выбираются инструменты.</param>
    /// <param name="connector"></param>
    /// <param name="optionType">В зависимости от сигнала запрашивается разный спред:
    /// Call - для лонгового сигнала;
    /// Put - для шортового сигнала;</param>
    /// <returns></returns>
    private static string requestInstruments(double price,
        Container container,
        OptionTradingClass oc, IConnector connector, OptionType optionType)
    {
        var curStrike = oc.Strikes.MinBy(s => Math.Abs(s - price));
        var curStrikeIdx = oc.Strikes.FindIndex(s => s == curStrike);
        int buyStrikeIdx; int sellStrikeIdx;

        if (optionType == OptionType.Call)
        {
            buyStrikeIdx = curStrikeIdx + container.SpreadSettings!.LongStrikeShift;
            sellStrikeIdx = curStrikeIdx + container.SpreadSettings!.ShortStrikeShift;
        }
        else
        {
            buyStrikeIdx = curStrikeIdx - container.SpreadSettings!.LongStrikeShift;
            sellStrikeIdx = curStrikeIdx - container.SpreadSettings!.ShortStrikeShift;
        }

        connector
            .RequestOption(optionType, container.Instrument!, oc.Strikes[buyStrikeIdx], oc.ExpirationDate, out var buy)
            .RequestOption(optionType, container.Instrument!, oc.Strikes[sellStrikeIdx], oc.ExpirationDate, out var sell);

        if (buy == null)
            return $"Cant request BUY {optionType} with {oc.Strikes[buyStrikeIdx]} {oc.ExpirationDate}";
        if (sell == null)
            return $"Cant request SELL {optionType} with {oc.Strikes[sellStrikeIdx]} {oc.ExpirationDate}";

        return createSpread(buy, sell, container, connector);
    }
    public static string OpenSpread(Container container, double price, IConnector connector, bool isLong)
    {
        if (container.Instrument == null) return "Instrument is null!";
        if (container.OptionStrategySettings == null) return "No OptionStrategy settings!";
        if (container.SpreadSettings == null) return "No Spread settings";
        var oc = connector.GetOptionTradingClass(
            container.Instrument.Id,
            container.OptionStrategySettings.GetMinExpirationDate());
        if (oc == null) return "Cant find option class for request!";
        return requestInstruments(price, container, oc, connector, isLong ? OptionType.Call : OptionType.Put);
    }
}
