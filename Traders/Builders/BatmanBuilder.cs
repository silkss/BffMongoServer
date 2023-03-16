namespace Traders.Builders;

using Common.Types.Base;
using Common.Types.Instruments;
using Connectors;
using Strategies.BatmanStrategy;
using System.Linq;

public static class BatmanBuilder
{
    private static (BatmanLeg? leg, string message) _createLeg(
        OptionType type,
        BatmanContainer container, OptionTradingClass oc, IConnector connector,
        double price)
    {
        var parent = container.Instrument!;
        var settings = container.Settings!;
        
        var expirationDate = oc.ExpirationDate;

        double mainBuyStrike, mainSellStrike;
        double z_closureBuyStrike, z_closureSellStrike;

        if (type == OptionType.Call)
        {
            mainBuyStrike = price * (settings.BaseBuyStrikeShift / 100.0 + 1.0);
            mainSellStrike = price * (settings.BaseSellStrikeShift / 100.0 + 1.0);
            z_closureBuyStrike = price * (settings.ClosureBuyStikeShift / 100.0 + 1.0);
            z_closureSellStrike = price * (settings.ClosureSellStikeShift / 100.0 + 1.0);

            var ordered_strikes = oc.Strikes.OrderBy(s => s);

            mainBuyStrike = ordered_strikes.First(s => s >= mainBuyStrike);
            mainSellStrike = ordered_strikes.First(s => s >= mainSellStrike);
            z_closureBuyStrike = ordered_strikes.First(s => s >= z_closureBuyStrike);
            z_closureSellStrike = ordered_strikes.First(s => s >= z_closureSellStrike);
        }
        else
        {
            mainBuyStrike = price * (1.0 - settings.BaseBuyStrikeShift / 100.0);
            mainSellStrike = price * (1.0 - settings.BaseSellStrikeShift / 100.0);
            z_closureBuyStrike = price * (1.0 - settings.ClosureBuyStikeShift / 100.0);
            z_closureSellStrike = price * (1.0 - settings.ClosureSellStikeShift / 100.0);

            var ordered_strikes = oc.Strikes.OrderByDescending(s => s);

            mainBuyStrike = ordered_strikes.First(s => s <= mainBuyStrike);
            mainSellStrike = ordered_strikes.First(s => s <= mainSellStrike);
            z_closureBuyStrike = ordered_strikes.First(s => s <= z_closureBuyStrike);
            z_closureSellStrike = ordered_strikes.First(s => s <= z_closureSellStrike);
        }
        connector
            .RequestOption(type, parent, mainBuyStrike, expirationDate, out var buyBasis)
            .RequestOption(type, parent, mainSellStrike, expirationDate, out var sellBasis)
            .RequestOption(type, parent, z_closureBuyStrike, expirationDate, out var zClosureBuy)
            .RequestOption(type, parent, z_closureSellStrike, expirationDate, out var zClosureSell);

        if (buyBasis == null)
            return (null, $"cant request basis buy {type}");
        if (sellBasis == null)
            return (null, $"cant request basis sell {type}");
        if (zClosureBuy == null)
            return (null, $"cant request zClosure buy {type}");
        if (zClosureSell == null)
            return (null, $"cant request zClosure sell {type}");

        var leg = new BatmanLeg(buyBasis, sellBasis,
            zClosureBuy, zClosureSell, settings.Volume);

        return (leg, "Ok");
    }

    public static (bool isOk, string message) CreateAndAddContainer(BatmanContainer container, double price, IConnector connector)
    {
        if (container.Instrument == null) return (false, "Instrument is null");
        if (container.Settings == null) return (false, "Settings is null");

        var oc = connector.GetOptionTradingClass(container.Instrument.Id,
            container.Settings.GetMinExpirationDate());
        if (oc == null) return (false, "Cant find option clas for request");

        string message;
        (var callLeg, message) = _createLeg(OptionType.Call,
            container, oc, connector, price);

        if (callLeg == null)
            return (false, message);

        (var putleg, message) = _createLeg(OptionType.Put,
            container, oc, connector, price);

        if (putleg == null)
            return (false, message);

        var newStrategy = new BatmanOptionStrategy(price, callLeg, putleg);
        newStrategy.Start(connector);
        container.AddStrategy(newStrategy);
        return (true, "Strategy added");
    }
}
