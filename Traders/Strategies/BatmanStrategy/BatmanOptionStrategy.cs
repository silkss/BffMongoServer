namespace Traders.Strategies.BatmanStrategy;

using Connectors;

public class BatmanOptionStrategy
{
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

    public void Work(IConnector connector, BatmanSettings containerSettings, decimal basisPrice)
    {
        var priceShift = basisPrice / BasisPriceAtOpenMoment - 1;
        CallLeg?.Work(connector, containerSettings, priceShift > 1.5m);
        PutLeg?.Work(connector, containerSettings, priceShift < -1.5m);
    }

    public void Stop(IConnector connector)
    {
        CallLeg?.Stop(connector);
        PutLeg?.Stop(connector);
    }
    public decimal GetClosureCurrencyPnlWithCommission()
    {
        var pnl = 0m;
        if (CallLeg != null)
        {
            pnl += CallLeg.GetClosureCurrencyPnlWithCommission();
        }
        if (PutLeg != null)
        {
            pnl += PutLeg.GetClosureCurrencyPnlWithCommission();
        }
        return pnl;
    }
    public decimal GetBasisCurrencyPnlWithCommission()
    {
        var pnl = 0m;
        if (CallLeg != null)
        {
            pnl += CallLeg.GetBasisCurrencyPnlWithCommission();
        }
        if (PutLeg != null)
        {
            pnl += PutLeg.GetBasisCurrencyPnlWithCommission();
        }
        return pnl;
    }
    public decimal GetTotalCurrencyPnlWithCommission()
    {
        var pnl = 0m;
        if (CallLeg != null)
        {
            pnl += CallLeg.GetTotalCurrencyPnlWithCommission();
        }
        if (PutLeg != null)
        {
            pnl += PutLeg.GetTotalCurrencyPnlWithCommission();
        }
        return pnl;
    }
    public decimal GetTotalCurrencyPositionCost()
    {
        var positionCost = 0m;
        if (CallLeg != null)
        {
            positionCost += CallLeg.GetTotalCurrencyPositionCost();
        }
        if (PutLeg != null)
        {
            positionCost += PutLeg.GetTotalCurrencyPositionCost();
        }
        return positionCost;
    }
}
