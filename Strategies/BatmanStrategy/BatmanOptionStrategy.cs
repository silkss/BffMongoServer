namespace Strategies.BatmanStrategy;

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

    }
    public void Work(IConnector connector, BatmanSettings containerSettings, decimal basisPrice)
    {
    }
}
