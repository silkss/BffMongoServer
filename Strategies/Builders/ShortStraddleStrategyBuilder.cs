using Connectors;
using Instruments;
using Strategies.Builders.Helpers;
using Strategies.Settings.Straddle;
using Strategies.Strategies.TradeUnions;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Strategies.Builders;

public static class ShortStraddleStrategyBuilder 
{
    private static double getClosestStrike(decimal price, IEnumerable<double> strikes)
    {
        var d_price = Decimal.ToDouble(price);
        return strikes.MinBy(s => Math.Abs(s - d_price));
    }
    public static Straddle? Build(
        IConnector connector,
        Instrument instrument,
        ShortStraddleSettings SSSettigs,
        double? strike = null)
    {
        var otc = connector
            .GetOptionTradingClasses(instrument, SSSettigs.OptionClass)
            .OrderBy(o => o.ExpirationDate)
            .First(o => (DateTime.Now - o.ExpirationDate).Days > SSSettigs.DaysToExpiration);

        if (otc == null) return null;

        if (strike == null)
            return BuilderHelper.CreateStraddle(
                connector,
                instrument,
                getClosestStrike(instrument.Last, otc.Strikes),
                otc.ExpirationDate);

        return BuilderHelper.CreateStraddle(
                connector,
                instrument,
                strike.Value,
                otc.ExpirationDate);
    }
}
