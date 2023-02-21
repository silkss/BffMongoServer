//namespace Traders.Builders;

//using Common.Types.Base;
//using Common.Types.Instruments;
//using Connectors;

//public static class SpreadBuilder
//{
////    private static string createBatmanStrategy(
////        Instrument basisBuyCall, Instrument basisSellCall, Instrument zClosureBuyCall, Instrument zClosureSellCall,
////        Instrument basisBuyPut, Instrument basisSellPut, Instrument zClosureBuyPut, Instrument zClosureSellPut, 
////        Container container, IConnector connector)
////    {
////        int volume = container.OptionStrategySettings!.Volume;

////        var callStrategy = new OptionStrategy(TradeLogic.Open)
////            .CreateAndAddBuyTradeUnit(basisBuyCall, volume, TradeLogic.Open)
////            .CreateAndAddSellTradeUnit(basisSellCall, volume, TradeLogic.Close);

////        var zClosureCallStrategy = new OptionStrategy(TradeLogic.Close)
////            .CreateAndAddBuyTradeUnit(zClosureBuyCall, volume, TradeLogic.Close)
////            .CreateAndAddSellTradeUnit(zClosureSellCall, volume, TradeLogic.Close);
////        callStrategy.Closure = zClosureCallStrategy;

////        var putStrategy = new OptionStrategy(TradeLogic.Open)
////            .CreateAndAddBuyTradeUnit(basisBuyPut, volume);
////        var zClosurePutStrategy = new OptionStrategy(TradeLogic.Close)
////            .CreateAndAddBuyTradeUnit(zClosureBuyPut, volume)
////            .CreateAndAddSellTradeUnit(zClosureSellPut, volume);
////        putStrategy.Closure = zClosurePutStrategy;

////        var batmanStrategy = new BatmanOptionStrategy();
////        batmanStrategy.CallDirection = callStrategy;
////        batmanStrategy.PutDirection = putStrategy;

////        container.AddOptionStrategy(batmanStrategy.Start(connector));

////        return "Cant create BATMAN STRATEGY";
////    }
////    private static string createSpread(Instrument buy, Instrument sell, 
////        Instrument closureBuy, Instrument closureSell,
////        Container container, IConnector connector)
////    {
////        var volume = container.OptionStrategySettings!.Volume;
////        var basisSpread = new OptionStrategy(TradeLogic.Open)
////            .CreateAndAddBuyTradeUnit(buy, volume, TradeLogic.Open)
////            .CreateAndAddSellTradeUnit(sell, volume, TradeLogic.Open);

////        var closureSpread = new OptionStrategy(TradeLogic.Close)
////            .CreateAndAddBuyTradeUnit(closureBuy, volume, TradeLogic.Close)
////            .CreateAndAddSellTradeUnit(closureSell, volume, TradeLogic.Close);

////        basisSpread.Closure = closureSpread;

////        basisSpread.Start(connector);
//////        container.AddOptionStrategy(basisSpread);

////        return $"Spread added {basisSpread}";
////    }

//    /// <summary>
//    /// 
//    /// </summary>
//    /// <param name="price"></param>
//    /// <param name="container">Тут должны хранится настройки, 
//    /// согласно которым будут выбираться опционные инструменты.</param>
//    /// <param name="oc">Опционный торговый класс, внутри которого выбираются инструменты.</param>
//    /// <param name="connector"></param>
//    /// <param name="optionType">В зависимости от сигнала запрашивается разный спред:
//    /// Call - для лонгового сигнала;
//    /// Put - для шортового сигнала;</param>
//    /// <returns></returns>
//    private static string requestInstruments(double price,
//        Container container,
//        OptionTradingClass oc, IConnector connector)
//    {
//        var curStrikeIdx = oc.GetIdOfClosestStrike(price);
//        if (curStrikeIdx < 0) return "No Closest strike!";

//        int buyCallStrikeIdx; int sellCallStrikeIdx;
//        int zClosureCallBuyStrikeIdx; int zClosureCallSellStrikeIdx;

//        int buyPutStrikeIdx; int sellPutStrikeIdx;
//        int zCLosurePutBuyStrikeIdx; int zClosurePutSellStrikeIdx;

//        buyCallStrikeIdx = curStrikeIdx + container.SpreadSettings!.BuyStrikeShift;
//        sellCallStrikeIdx = curStrikeIdx + container.SpreadSettings!.SellStrikeShift;

//        buyPutStrikeIdx = curStrikeIdx - container.SpreadSettings!.BuyStrikeShift;
//        sellPutStrikeIdx = curStrikeIdx - container.SpreadSettings!.SellStrikeShift;

//        zClosureCallBuyStrikeIdx = curStrikeIdx + container.ClosureSpreadSettings!.BuyStrikeShift;
//        zClosureCallSellStrikeIdx = curStrikeIdx + container.ClosureSpreadSettings!.SellStrikeShift;
//        zCLosurePutBuyStrikeIdx = curStrikeIdx - container.ClosureSpreadSettings!.BuyStrikeShift;
//        zClosurePutSellStrikeIdx = curStrikeIdx - container.ClosureSpreadSettings!.SellStrikeShift;

//        var expirationDate = oc.ExpirationDate;

//        connector
//            .RequestCall(container.Instrument!,
//                oc.Strikes[buyCallStrikeIdx], expirationDate, out var buyBasisCall)
//            .RequestCall(container.Instrument!,
//                oc.Strikes[sellCallStrikeIdx], expirationDate, out var sellBasisCall)
//            .RequestCall(container.Instrument!,
//                oc.Strikes[zClosureCallBuyStrikeIdx], expirationDate, out var zClosureCallBuy)
//            .RequestCall(container.Instrument!,
//                oc.Strikes[zClosureCallSellStrikeIdx], expirationDate, out var zClosureCallSell);

//        connector
//            .RequestPut(container.Instrument!,
//                oc.Strikes[buyPutStrikeIdx], expirationDate, out var buyBasisPut)
//            .RequestPut(container.Instrument!,
//                oc.Strikes[sellPutStrikeIdx], expirationDate, out var sellBasisPut)
//            .RequestPut(container.Instrument!,
//                oc.Strikes[zCLosurePutBuyStrikeIdx], expirationDate, out var zCLosurePutBuy)
//            .RequestPut(container.Instrument!,
//                oc.Strikes[zClosurePutSellStrikeIdx], expirationDate, out var zClosurePutSell);


//        if (buyBasisCall == null)
//            return $"Cant request BASIS BUY CALL with " +
//                $"{oc.Strikes[buyCallStrikeIdx]} {expirationDate}";

//        if (sellBasisCall == null)
//            return $"Cant request BASIS SELL CALL with " +
//                $"{oc.Strikes[sellCallStrikeIdx]} {expirationDate}";

//        if (zClosureCallBuy == null)
//            return $"Cant request Z-CLOSURE BUY CALL with " +
//                $"{oc.Strikes[zClosureCallBuyStrikeIdx]} {expirationDate}";

//        if (zClosureCallSell == null)
//            return $"Cant request Z-CLOSURE SELL CALL with " +
//                $"{oc.Strikes[zClosureCallSellStrikeIdx]} {expirationDate}";

//        if (buyBasisPut == null)
//            return $"Cant request BASIS BUY PUT with " +
//                $"{oc.Strikes[buyPutStrikeIdx]} {expirationDate}";

//        if (sellBasisPut == null)
//            return $"Cant request BASIS SELL PUT with " +
//                $"{oc.Strikes[sellPutStrikeIdx]} {expirationDate}";

//        if (zCLosurePutBuy == null)
//            return $"Cant request Z-CLOSURE BUY PUT with " +
//                $"{oc.Strikes[buyPutStrikeIdx]} {expirationDate}";

//        if (zClosurePutSell == null)
//            return $"Cant request Z-CLOSURE BUY PUT with " +
//                $"{oc.Strikes[zClosurePutSellStrikeIdx]} {expirationDate}";

//        return createBatmanStrategy(
//            buyBasisCall, zClosureCallBuy, zClosureCallSell,
//            buyBasisPut, zCLosurePutBuy, zClosurePutSell,
//            container, connector);
//    }
//    public static string OpenSpread(Container container, double price, IConnector connector)
//    {
//        if (container.Instrument == null) return "Instrument is null!";
//        if (container.OptionStrategySettings == null) return "No OptionStrategy settings!";
//        if (container.SpreadSettings == null) return "No Spread settings";
//        if (container.ClosureSpreadSettings == null) return "No ClosureSpread settings";
//        var oc = connector.GetOptionTradingClass(
//            container.Instrument.Id,
//            container.OptionStrategySettings.GetMinExpirationDate());
//        if (oc == null) return "Cant find option class for request!";
//        return requestInstruments(price, container, oc, connector);
//    }
//}
