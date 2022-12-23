using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Types.Instruments;

public class OptionTradingClass
{
    public OptionTradingClass(
        int underlyingConId, string exchange, string tradingClass, int multiplier,
        DateTime expirationDate, IEnumerable<double> strikes)
    {
        UnderlyingConId = underlyingConId;
        Exchange = exchange;
        TradingClass = tradingClass;
        Multiplier = multiplier;
        ExpirationDate = expirationDate;
        Strikes = strikes.ToList();
    }
    public int UnderlyingConId { get; set; }
    public string Exchange { get; set; }
    public string TradingClass { get; set; }
    public int Multiplier { get; set; }
    public DateTime ExpirationDate { get; set; }
    public List<double> Strikes { get; set; }
}
