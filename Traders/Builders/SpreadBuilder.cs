using Common.Types.Base;
using Common.Types.Instruments;
using Connectors;
using Strategies;
using Strategies.TradeUnits;
using System;
using System.Linq;

namespace Traders.Builders;

public static class SpreadBuilder
{
    private static OptionStrategy createShortSpread(Instrument buy, Instrument sell, int volume)
    {
        var spread = new OptionStrategy()
        {
            Logic = TradeLogic.Open
        };
        var buyUnit = new OptionTradeUnit
        {
            Logic = TradeLogic.Open,
            Instrument = buy,
            Direction = Directions.Buy,
            Volume = volume,

        };
        var sellUnit = new OptionTradeUnit
        {
            Logic = TradeLogic.Open,
            Instrument = sell,
            Direction = Directions.Sell,
            Volume = volume,
        };
        spread.AddTradeUnit(buyUnit);
        spread.AddTradeUnit(sellUnit);
        return spread;
    }

    /// <summary>
    /// Шорт. Необходимо купить пут, со страйком близким к цене(price)
    //  продать пут со страйком НИЖЕ цены. Насколько ниже должно быть указано в настройках.
    /// </summary>
    /// <param name="container"></param>
    /// <param name="price"></param>
    /// <param name="connector"></param>
    /// <returns></returns>
    public static string OpenShortSpread(Container container, double price, IConnector connector)
    {
        if (container.Instrument == null)
        {
            return "Instument is null!";
        }
        var parent = container.Instrument;

        if (container.OptionStrategySettings == null)
        {
            return "No option strategy settings!"; 
        }

        var oc = connector.GetOptionTradingClass(
            parent.Id,
            container.OptionStrategySettings.GetMinExpirationDate());
            
        if (oc == null)
        {
            return "Cant find option class for request!";
        }

        var buyStrike = oc.Strikes.MinBy(s => Math.Abs(s - price));
        var buyStrikeIdx = oc.Strikes.FindIndex(s => s == buyStrike);
        var sellStrike = oc.Strikes[buyStrikeIdx - 4];

        connector
             .RequestPut(parent, buyStrike, oc.ExpirationDate, out var buyPut)
             .RequestPut(parent, sellStrike, oc.ExpirationDate, out var sellPut);

        if (buyPut == null)
            return $"Cant reqste PUT with {buyStrike} {oc.ExpirationDate}";
        if (sellPut == null)
            return $"Cant reqste PUT with {sellStrike} {oc.ExpirationDate}";
        var spread = createShortSpread(buyPut, sellPut, container.OptionStrategySettings.Volume);
        spread.Start(connector);
        container.AddOptionStrategy(spread);

        return $"Added spread buy {buyStrike}, sell {sellStrike} exp. {oc.ExpirationDate}";
    }
}
