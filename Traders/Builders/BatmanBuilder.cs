namespace Traders.Builders;

using Common.Types.Base;
using Common.Types.Instruments;
using Connectors;
using Strategies.BatmanStrategy;

public static class BatmanBuilder
{
    private static (BatmanLeg? leg, string message) _createLeg(
        OptionType type,
        BatmanContainer container, OptionTradingClass oc, IConnector connector,
        int closestStrikeId)
    {
        var parent = container.Instrument!;
        var settings = container.Settings!;

        var expirationDate = oc.ExpirationDate;

        int baseBuyStrikeId;
        int baseSellStrikeId;

        int closureBuyStrikeId;
        int closureSellStrikeId;

        if (type == OptionType.Call)
        {
            baseBuyStrikeId = closestStrikeId + settings.BaseBuyStrikeShift;
            baseSellStrikeId = closestStrikeId + settings.BaseSellStrikeShift;
            closureBuyStrikeId = closestStrikeId + settings.ClosureBuyStikeShift;
            closureSellStrikeId = closestStrikeId + settings.ClosureSellStikeShift;
        }
        else
        {
            baseBuyStrikeId = closestStrikeId - settings.BaseBuyStrikeShift;
            baseSellStrikeId = closestStrikeId - settings.BaseSellStrikeShift;
            closureBuyStrikeId = closestStrikeId - settings.ClosureBuyStikeShift;
            closureSellStrikeId = closestStrikeId - settings.ClosureSellStikeShift;
        }
        connector
            .RequestOption(type, parent, oc.Strikes[baseBuyStrikeId],
                expirationDate, out var buyBasis)
            .RequestOption(type,parent, oc.Strikes[baseSellStrikeId],
                expirationDate, out var sellBasis)
            .RequestOption(type, parent, oc.Strikes[closureBuyStrikeId],
                expirationDate, out var zClosureBuy)
            .RequestOption(type, parent, oc.Strikes[closureSellStrikeId],
                expirationDate, out var zClosureSell);

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

        var closestStrikeId = oc.GetIdOfClosestStrike(price);
        if (closestStrikeId < 0)
            return (false, "Cant find closes strike id!");

        string message;
        (var callLeg, message) = _createLeg(OptionType.Call,
            container, oc, connector, closestStrikeId);

        if (callLeg == null)
            return (false, message);

        (var putleg, message) = _createLeg(OptionType.Put,
            container, oc, connector, closestStrikeId);

        if (putleg == null)
            return (false, message);

        var newStrategy = new BatmanOptionStrategy(price, callLeg, putleg);
        newStrategy.Start(connector);
        container.AddStrategy(newStrategy);
        return (true, "Strategy added");
    }
}
