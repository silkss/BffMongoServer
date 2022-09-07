using System;
using System.Collections.Generic;

namespace ContainerStore.Data.Models.Instruments;

public class OptionTradingClass
{
    public OptionTradingClass(
        int underlyingConId, string exchange, string tradingClass, int multiplier, 
        DateTime expirationDate, IEnumerable<decimal> strikes)
    {
        UnderlyingConId = underlyingConId;
        Exchange = exchange;
        TradingClass = tradingClass;
        Multiplier = multiplier;
        ExpirationDate = expirationDate;
        Strikes = strikes;
    }
    public int UnderlyingConId { get; set; }
    public string Exchange { get; set; }
    public string TradingClass { get; set; }
    public int Multiplier { get; set; }
    public DateTime ExpirationDate { get; set; }
    public IEnumerable<decimal> Strikes { get; set; }
}
