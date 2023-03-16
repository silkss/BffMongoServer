namespace Traders.Strategies.BatmanStrategy;

using Connectors;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization.Attributes;
using System;

public class BatmanOptionStrategy
{
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string? Id { get; set; }
    public DateTime CreationTime { get; set; }

    public BatmanOptionStrategy() { }
    public BatmanOptionStrategy(double basisPrice, BatmanLeg callLeg, BatmanLeg putLeg)
    {
        BasisPriceAtOpenMoment = Common.Helpers.MathHelper.ConvertDoubleToDecimal(basisPrice);
        CallLeg = callLeg;
        PutLeg = putLeg;
    }
    public decimal BasisPriceAtOpenMoment { get; set; }
    public BatmanLeg? CallLeg { get; set; }
    public BatmanLeg? PutLeg { get; set; }
    public void Start(IConnector connector)
    {
        CallLeg?.Start(connector);
        PutLeg?.Start(connector);
    }

    public void Work(IConnector connector, ILogger<ContainerTrader> logger, BatmanSettings containerSettings, decimal basisPrice)
    {
        if (basisPrice == 0m)
            return;
        var priceShift = (basisPrice / BasisPriceAtOpenMoment - 1) * 100;
        CallLeg?.Work(connector, logger, containerSettings, priceShift > 2m, priceShift < 0);
        PutLeg?.Work(connector, logger, containerSettings, priceShift < -2m, priceShift > 0);
    }

    public void Close() {
        CallLeg?.Close();
        PutLeg?.Close();
    }

    public void Stop(IConnector connector)
    {
        CallLeg?.Stop(connector);
        PutLeg?.Stop(connector);
    }
    public decimal GetClosureCurrencyTheorPnlWithCommission() {
        var pnl = 0m;
        if (CallLeg != null) {
            pnl += CallLeg.GetClosureCurrencyTheorPnlWithCommission();
        }
        if (PutLeg != null) {
            pnl += PutLeg.GetClosureCurrencyTheorPnlWithCommission();
        }
        return pnl;
    }

    public bool IsClosed() {
        if (PutLeg != null) {
            var isdone = PutLeg.IsClosed();
            if (!isdone) return isdone;
        }
        if (CallLeg != null) {
            return CallLeg.IsClosed();
        }
        return false;
    }

    public decimal GetMainCurrencyTheorPnlWithCommission()
    {
        var pnl = 0m;
        if (CallLeg != null)
        {
            pnl += CallLeg.GetMainCurrencyTheorPnlWithCommission();
        }
        if (PutLeg != null)
        {
            pnl += PutLeg.GetMainCurrencyTheorPnlWithCommission();
        }
        return pnl;
    }
    public decimal GetTotalCurrencyBidAskPnlWithCommission()
    {
        var pnl = 0m;
        if (CallLeg != null)
        {
            pnl += CallLeg.GetTotalCurrencyBidAskPnlWithCommission();
        }
        if (PutLeg != null)
        {
            pnl += PutLeg.GetTotalCurrencyBidAskPnlWithCommission();
        }
        return pnl;
    }
    public decimal GetTotalCurrencyTheorPnlWithCommission() {
        var pnl = 0m;
        if (CallLeg != null) {
            pnl += CallLeg.GetTotalCurrencyTheorPnlWithCommission();
        }
        if (PutLeg != null) {
            pnl += PutLeg.GetTotalCurrencyTheorPnlWithCommission();
        }
        return pnl;
    }
    public decimal GetTotalCurrencyPositionCost()
    {
        var positionCost = 0m;
        if (CallLeg != null) {
            positionCost += CallLeg.GetTotalCurrencyPositionCost();
        }
        if (PutLeg != null) {
            positionCost += PutLeg.GetTotalCurrencyPositionCost();
        }
        return positionCost;
    }
}
