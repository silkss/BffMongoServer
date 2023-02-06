namespace Strategies.TradeUnits.Spreads.Base;

using Common.Types.Base;
using Common.Types.Instruments;
using Connectors;
using Strategies.Settings;
using System;
using System.ComponentModel;
using System.Linq;

public class Spread
{
    public OptionTradeUnit LongOptionUnit { get; set; }
    public OptionTradeUnit ShortOptionUnit { get; set; }

    public static string Create(IConnector connector,
        OptionTradingClass tradingClass, Instrument parent,
        double basisPrice,
        SpreadSettings settings, OptionStrategySettings optionStrategySettings,
        OptionType longOptionType, OptionType shortOptionType, out Spread? spread)
    {
        spread = null;
        var closestStrike = tradingClass.Strikes.MinBy(s => Math.Abs(s - basisPrice));
        var closestStrikeIdx = tradingClass.Strikes.FindIndex(s => s == closestStrike);

        int longStrikeIdx; int shortStrikeIdx;

        if (longOptionType == OptionType.Call)
        {
            longStrikeIdx = closestStrikeIdx + settings.LongStrikeShift;
        }
        else
        {
            longStrikeIdx = closestStrikeIdx - settings.LongStrikeShift;
        }

        if (shortOptionType == OptionType.Call)
        {
            shortStrikeIdx = closestStrikeIdx + settings.ShortStrikeShift;
        }
        else
        {
            shortStrikeIdx = closestStrikeIdx - settings.ShortStrikeShift;
        }
        connector
            .RequestOption(
                longOptionType, 
                parent, 
                tradingClass.Strikes[longStrikeIdx], 
                tradingClass.ExpirationDate, out var longOption)
            .RequestOption(
                shortOptionType, 
                parent, 
                tradingClass.Strikes[shortStrikeIdx], 
                tradingClass.ExpirationDate, out var shortOption);

        if (longOption == null)
            return $"Cant request BUY {longOptionType} with {tradingClass.Strikes[longStrikeIdx]} {tradingClass.ExpirationDate}";
        if (shortOption == null)
            return $"Cant request SELL {shortOptionType} with {tradingClass.Strikes[shortStrikeIdx]} {tradingClass.ExpirationDate}";

        spread = new Spread
        {
            LongOptionUnit = new OptionTradeUnit(longOption, Directions.Buy, optionStrategySettings.Volume),
            ShortOptionUnit = new OptionTradeUnit(shortOption, Directions.Sell, optionStrategySettings.Volume),
        };
        return "Created";
    }
}
