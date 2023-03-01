namespace Traders.Strategies.BatmanStrategy;

using Common.Types.Base;
using Common.Types.Instruments;
using Connectors;
using Traders.Strategies.Base;

public class BatmanLeg
{
    public BatmanLeg(Instrument buy, Instrument sell, Instrument closureBuy, Instrument closureSell, int volume)
    {
        BuyLeg = new OptionTradeUnit(buy, Directions.Buy, volume, TradeLogic.Open);
        SellLeg = new OptionTradeUnit(sell, Directions.Sell, volume, TradeLogic.Close);

        ClosureBuyLeg = new OptionTradeUnit(closureBuy, Directions.Buy, volume, TradeLogic.Open);
        ClosureSellLeg = new OptionTradeUnit(closureSell, Directions.Sell, volume, TradeLogic.Open);
    }
    public TradeLogic Logic { get; set; }
    public OptionTradeUnit BuyLeg { get; set; }
    public OptionTradeUnit SellLeg { get; set; }

    public OptionTradeUnit ClosureBuyLeg { get; set; }
    public OptionTradeUnit ClosureSellLeg { get; set; }

    public void Start(IConnector connector)
    {
        BuyLeg.Start(connector);
        SellLeg.Start(connector);

        ClosureBuyLeg.Start(connector);
        ClosureSellLeg.Start(connector);
    }

    public void Work(IConnector connector, BatmanSettings settings, bool isPriceShifted)
    {
        BuyLeg.Work(connector, settings.Account!, settings.OrderPriceShift);
        SellLeg.Work(connector, settings.Account!, settings.OrderPriceShift);
        ClosureBuyLeg.Work(connector, settings.Account!, settings.OrderPriceShift);
        ClosureSellLeg.Work(connector, settings.Account!, settings.OrderPriceShift);

        if (Logic == TradeLogic.Open)
        {
            if (BuyLeg.IsDone())
            {
                if (isPriceShifted && SellLeg.Logic == TradeLogic.Close)
                {
                    SellLeg.Logic = TradeLogic.Open;
                }
            }
        }
        if (ClosureBuyLeg.Logic == TradeLogic.Open && ClosureSellLeg.Logic == TradeLogic.Open)
        {
            if (GetClosureCurrencyPnlWithCommission() > ClosureBuyLeg.EnterPriceWithCommission &&
                ClosureBuyLeg.EnterPriceWithCommission != 0m)
            {
                ClosureBuyLeg.Logic = TradeLogic.Close;
                ClosureSellLeg.Logic = TradeLogic.Close;
            }
        }
        if (isPriceShifted)
        {
            var positionCost = GetTotalCurrencyPositionCost();

            if (positionCost != 0 && GetTotalCurrencyPnlWithCommission() > GetTotalCurrencyPositionCost() * 0.8m)
            {
                SetLogic(TradeLogic.Close);
            }
        }
    }
    public void SetLogic(TradeLogic logic)
    {
        Logic = logic;
        BuyLeg.Logic = logic;
        SellLeg.Logic = logic;
        ClosureBuyLeg.Logic = logic;
        ClosureSellLeg.Logic = logic;
    }
    public void Stop(IConnector connector)
    {
        BuyLeg.Stop(connector);
        SellLeg.Stop(connector);
        ClosureBuyLeg.Stop(connector);
        ClosureSellLeg.Stop(connector);
    }

    public decimal GetClosureBuyLegPositionPrice() => ClosureBuyLeg.EnterPriceWithCommission;

    public decimal GetClosureCurrencyPnlWithCommission() =>
        ClosureBuyLeg.GetCurrencyPnlWithCommission() + ClosureSellLeg.GetCurrencyPnlWithCommission();

    public decimal GetBasisCurrencyPnlWithCommission() =>
        BuyLeg.GetCurrencyPnlWithCommission() + SellLeg.GetCurrencyPnlWithCommission();

    public decimal GetTotalCurrencyPnlWithCommission()
    {
        var pnl = 0m;
        pnl += BuyLeg.GetCurrencyPnlWithCommission();
        pnl += SellLeg.GetCurrencyPnlWithCommission();
        pnl += ClosureBuyLeg.GetCurrencyPnlWithCommission();
        pnl += ClosureSellLeg.GetCurrencyPnlWithCommission();
        return pnl;
    }
    public decimal GetTotalCurrencyPositionCost()
    {
        var positionCost = 0m;
        positionCost += BuyLeg.EnterPriceWithCommission;
        positionCost += SellLeg.EnterPriceWithCommission;
        positionCost += ClosureBuyLeg.EnterPriceWithCommission;
        positionCost += ClosureSellLeg.EnterPriceWithCommission;
        return positionCost;
    }
}
