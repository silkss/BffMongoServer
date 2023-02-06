namespace Strategies.TradeUnits.Spreads;

using Common.Types.Base;
using Common.Types.Instruments;
using Connectors;
using Strategies.Settings;
using Strategies.TradeUnits.Spreads.Base;

public class SpreadWithHedgeSpread
{
    public TradeLogic Logic { get; set; }
    public Spread? BaseSpread { get; set; }
    public Spread? HedgeSpread { get; set; }

    public static string Create(IConnector connector,
        OptionTradingClass tradingClass, Instrument parent, double basisPrice,
        SpreadWithHedgeSpreadSettings settings,
        OptionStrategySettings optionStrategySettings,
        bool isLong, out SpreadWithHedgeSpread? creating)
    {
        creating = null;
        if (isLong)
        {
            var msg = Spread.Create(
                connector, tradingClass, parent,
                basisPrice,
                settings.BaseSpreadSettings,
                optionStrategySettings,
                OptionType.Call, OptionType.Call, out var baseSpread);
            if (baseSpread == null)
                return msg;

            msg = Spread.Create(
                connector, tradingClass, parent,
                basisPrice, settings.HedgeSpreadSettings,
                optionStrategySettings,
                OptionType.Call, OptionType.Call, out var hedgeSpread);
            if (hedgeSpread == null)
                return msg;
            creating = new SpreadWithHedgeSpread
            {
                BaseSpread = baseSpread,
                HedgeSpread = hedgeSpread,
                Logic = TradeLogic.Open
            };
        }

        return "Created";
    }
}
