namespace Strategies.BatmanStrategy;

using Common.Types.Base;
using Common.Types.Instruments;
using Connectors;
using Strategies.Base;

public class BatmanLeg
{
    public BatmanLeg(Instrument buy, Instrument sell, Instrument closureBuy, Instrument closureSell, int volume)
    {
        BuyLeg = new OptionTradeUnit(buy, Directions.Buy, volume, TradeLogic.Open);
        SellLeg = new OptionTradeUnit(sell, Directions.Sell, volume, TradeLogic.Close);

        ClosureBuyLeg = new OptionTradeUnit(closureBuy, Directions.Buy, volume, TradeLogic.Close);
        ClosureSellLeg = new OptionTradeUnit(closureSell, Directions.Sell, volume, TradeLogic.Close);
    }
    public TradeLogic Logic { get; set; }
    public OptionTradeUnit BuyLeg { get; set; }
    public OptionTradeUnit SellLeg { get; set; }

    public OptionTradeUnit ClosureBuyLeg { get; set; }
    public OptionTradeUnit ClosureSellLeg { get; set; }

    public void Start(IConnector connector)
    {

    }
    public void Work(IConnector connector, BatmanSettings settings, decimal basisPriceShift)
    {

    }
}
