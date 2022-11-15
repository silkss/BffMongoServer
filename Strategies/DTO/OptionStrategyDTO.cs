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
    public int Position { get; set; }
    public Logic Logic { get; set; }
    public List<Transaction>? Orders { get; set; }
}

public static class OptionStrategyDtoExtension
{
    public static OptionStrategyDTO ToDto(this OptionStrategy strategy) => new OptionStrategyDTO
    {
        Instrument = strategy.Instrument,
        CurrencyPnL = strategy.GetCurrencyPnl(),
        Position = strategy.GetPosition(),
        Logic = strategy.Logic,
        Orders = strategy.Orders,
    };
}