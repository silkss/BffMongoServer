namespace Traders.Strategies.BatmanStrategy;

using Amazon.Runtime.Internal.Util;
using Common.Types.Base;
using Common.Types.Instruments;
using Connectors;
using Microsoft.Extensions.Logging;
using System;
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

    public void Work(IConnector connector, ILogger<ContainerTrader> logger, BatmanSettings settings, 
        bool isPriceShifted, 
        bool isPriceOpposite)
    {
        BuyLeg.Work(connector, logger, settings.Account!, settings.OrderPriceShift);
        SellLeg.Work(connector, logger, settings.Account!, settings.OrderPriceShift);
        ClosureBuyLeg.Work(connector, logger, settings.Account!, settings.OrderPriceShift);
        ClosureSellLeg.Work(connector, logger, settings.Account!, settings.OrderPriceShift);

        if (Logic == TradeLogic.Open)
        {
            if (BuyLeg.Logic == TradeLogic.Open)
            {
                if (isPriceShifted && SellLeg.Logic == TradeLogic.Close)
                {
                    var optionType = SellLeg.Instrument.OptionType;
                    var strike = SellLeg.Instrument.Strike;
                    logger.LogInformation("Цена сместилась, включаю продажную ногу {optionType} {strike}", optionType, strike);
                    SellLeg.Logic = TradeLogic.Open;
                }
            }
        }
        if (ClosureBuyLeg.Logic == TradeLogic.Open && ClosureSellLeg.Logic == TradeLogic.Open)
        {
            var pnl = GetClosureCurrencyPnlWithCommission();
            var enterPrice = Math.Abs(ClosureBuyLeg.EnterPriceWithCommission);

            if (pnl > enterPrice * 1.2m &&
                ClosureBuyLeg.EnterPriceWithCommission != 0m)
            {
                var optionType = ClosureBuyLeg.Instrument.OptionType;

                logger.LogInformation("PnL достигала 120% цены покупного опциона.\n" +
                    "Закрываю Z-Closure {optionType}" +
                    "\nPnl:{pnl}\nEnterPrice{enterPrice}",optionType, pnl, enterPrice);

                ClosureBuyLeg.Logic = TradeLogic.Close;
                ClosureSellLeg.Logic = TradeLogic.Close;
            }
        }
        if (isPriceOpposite)
        {
            if (Logic == TradeLogic.Close)
            {
                return;
            }

            var positionCost = GetTotalCurrencyPositionCost();

            var pnl = GetTotalCurrencyPnlWithCommission();
            var optionType = ClosureBuyLeg.Instrument.OptionType;

            if (positionCost > 0 && pnl > positionCost * 0.8m)
            {
                
                logger.LogInformation("PnL выше 80% цены всей конcтрукции\n" +
                    "Закрываю \"{optionType} Крыло Бэтмана\"\n" +
                    "PnL: {pnl}\n" +
                    "Position Cost: {positionCost}", optionType, pnl, positionCost);
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

    public decimal GetMainCurrencyPnlWithCommission() =>
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
