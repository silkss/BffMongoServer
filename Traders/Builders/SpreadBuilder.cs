namespace Traders.Builders;

using System;
using System.Linq;
using Common.Types.Base;
using Common.Types.Instruments;
using Connectors;
using Strategies;

public static class SpreadBuilder
{
    private static string createSpread(Instrument buy, Instrument sell, 
        Instrument closureBuy, Instrument closureSell,
        Container container, IConnector connector)
    {
        var basisSpread = new OptionStrategy() { Logic = TradeLogic.Open };
        basisSpread.AddTradeUnit(new(buy, Directions.Buy, container.OptionStrategySettings!.Volume));
        basisSpread.AddTradeUnit(new(sell, Directions.Sell, container.OptionStrategySettings!.Volume));

        var closureSpread = new OptionStrategy() { Logic = TradeLogic.Close };
        closureSpread.AddTradeUnit(new(closureBuy, Directions.Buy, container.OptionStrategySettings!.Volume));
        closureSpread.AddTradeUnit(new(closureSell, Directions.Sell, container.OptionStrategySettings!.Volume));

        basisSpread.Closure = closureSpread;

        basisSpread.Start(connector);
        container.AddOptionStrategy(basisSpread);

        return $"Spread added {basisSpread}";
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
        var curStrikeIdx = oc.GetIdOfClosestStrike(price);
        if (curStrikeIdx < 0) return "No Closest strike!";

        int buyStrikeIdx; int sellStrikeIdx;
        int closureBuyStrikeIdx; int closureSellStrikeIdx;

        if (optionType == OptionType.Call)
        {
            buyStrikeIdx = curStrikeIdx + container.SpreadSettings!.BuyStrikeShift;
            closureBuyStrikeIdx = curStrikeIdx + container.ClosureSpreadSettings!.BuyStrikeShift;
            sellStrikeIdx = curStrikeIdx + container.SpreadSettings!.SellStrikeShift;
            closureSellStrikeIdx = curStrikeIdx + container.ClosureSpreadSettings!.SellStrikeShift;
        }
        else
        {
            buyStrikeIdx = curStrikeIdx - container.SpreadSettings!.BuyStrikeShift;
            closureBuyStrikeIdx = curStrikeIdx - container.ClosureSpreadSettings!.BuyStrikeShift;

            sellStrikeIdx = curStrikeIdx - container.SpreadSettings!.SellStrikeShift;
            closureSellStrikeIdx = curStrikeIdx - container.ClosureSpreadSettings!.SellStrikeShift;
        }

        connector
            .RequestOption(optionType, container.Instrument!, oc.Strikes[buyStrikeIdx], oc.ExpirationDate, out var buy)
            .RequestOption(optionType, container.Instrument!, oc.Strikes[closureBuyStrikeIdx], oc.ExpirationDate, out var closureBuy)
            .RequestOption(optionType, container.Instrument!, oc.Strikes[sellStrikeIdx], oc.ExpirationDate, out var sell)
            .RequestOption(optionType, container.Instrument!, oc.Strikes[closureSellStrikeIdx], oc.ExpirationDate, out var closureSell);


        if (buy == null)
            return $"Cant request BUY {optionType} with {oc.Strikes[buyStrikeIdx]} {oc.ExpirationDate}";
        if (closureBuy == null)
            return $"Cant request BUY {optionType} with {oc.Strikes[closureBuyStrikeIdx]} {oc.ExpirationDate}";
        if (sell == null)
            return $"Cant request SELL {optionType} with {oc.Strikes[sellStrikeIdx]} {oc.ExpirationDate}";
        if (closureSell == null)
            return $"Cant request SELL {optionType} with {oc.Strikes[closureSellStrikeIdx]} {oc.ExpirationDate}";

        return createSpread(buy, sell, closureBuy, closureSell, container, connector);
    }
    public static string OpenSpread(Container container, double price, IConnector connector, bool isLong)
    {
        if (container.Instrument == null) return "Instrument is null!";
        if (container.OptionStrategySettings == null) return "No OptionStrategy settings!";
        if (container.SpreadSettings == null) return "No Spread settings";
        if (container.ClosureSpreadSettings == null) return "No ClosureSpread settings";
        var oc = connector.GetOptionTradingClass(
            container.Instrument.Id,
            container.OptionStrategySettings.GetMinExpirationDate());
        if (oc == null) return "Cant find option class for request!";
        return requestInstruments(price, container, oc, connector, isLong ? OptionType.Call : OptionType.Put);
    }
}
