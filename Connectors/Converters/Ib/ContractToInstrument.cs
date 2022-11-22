using Common.Enums;
using Common.Helpers;
using IBApi;
using Instruments;
using System;
using System.Globalization;
using System.Linq;

namespace Connectors.Converters.Ib;

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
        MarketRuleId = int.Parse(contract.MarketRuleIds.Split(',').First()),
        LastTradeDate = DateTime
            .ParseExact(contract.Contract.LastTradeDateOrContractMonth, "yyyyMMdd", CultureInfo.CurrentCulture),
        Strike = Helper.ConvertDoubleToDecimal(contract.Contract.Strike),
        OptionType = contract.Contract.Right == "C" ? OptionType.Call : OptionType.Put
    };
    public static Contract ToIbContract(this Instrument instrument) => new Contract
    {
        ConId = instrument.Id,
        Symbol = instrument.Symbol,
        TradingClass = instrument.TradeClass,
        LocalSymbol = instrument.FullName,
        Multiplier = instrument.Multiplier.ToString(),
        Exchange = instrument.Exchange,
        Currency = instrument.Currency,
        SecType = instrument.Type == InstrumentType.Future ? "FUT" : "FOP",
        Strike = Convert.ToDouble(instrument.Strike),
        LastTradeDateOrContractMonth = instrument.LastTradeDate.ToString("yyyyMMdd"),
        Right = instrument.Type == InstrumentType.Future ? null : 
            instrument.OptionType == OptionType.Call ? "C" : "R",
    };
}
