using ContainerStore.Common.Enums;
using ContainerStore.Connectors.Helpers;
using ContainerStore.Data.Models;
using IBApi;
using System;
using System.Globalization;

namespace ContainerStore.Connectors.Converters.Ib;

internal static class ContractToInstrument
{
    public static Instrument ToInstrument(this ContractDetails contract) => new Instrument
    {
        Id = contract.Contract.ConId,
        Type = Helper.IbSecTypeToBffEnum(contract.Contract.SecType),
        Symbol = contract.Contract.Symbol,
        MinTick = Helper.ConvertDoubleToDecimal(contract.MinTick),
        FullName = contract.Contract.LocalSymbol,
        Exchange = contract.Contract.Exchange,
        Currency = contract.Contract.Currency,
        TradeClass = contract.Contract.TradingClass,
        Multiplier = int.Parse(contract.Contract.Multiplier),
        LastTradeDate = DateTime
            .ParseExact(contract.Contract.LastTradeDateOrContractMonth, "yyyyMMdd", CultureInfo.CurrentCulture),
        Strike = Helper.ConvertDoubleToDecimal(contract.Contract.Strike),
        OptionType = contract.Contract.Right == "C" ? OptionType.Call : OptionType.Put
    };
}
