using Instruments;
using Strategies.Depend;
using Strategies.Enums;
using System.Collections.Generic;
using Transactions;

namespace Strategies.DTO;

public class OptionStrategyDTO
{
    public Instrument? Instrument { get; set; }
    public decimal CurrencyPnL { get; set; }
    public decimal OpenPrice { get; set; }
    public int Position { get; set; }
    public Logic Logic { get; set; }
    public List<Transaction>? Orders { get; set; }
}

public static class OptionStrategyDtoExtension
{
    public static OptionStrategyDTO ToDto(this OptionStrategy strategy) => new OptionStrategyDTO
    {
        CurrencyPnL = strategy.GetCurrencyPnl(),
        Position = strategy.GetPosition(),
        Instrument = strategy.Instrument,
        OpenPrice = strategy.OpenPrice,
        Orders = strategy.Orders,
        Logic = strategy.Logic,
    };
}