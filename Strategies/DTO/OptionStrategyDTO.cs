using Instruments;
using Strategies.Depend;

namespace Strategies.DTO;

public class OptionStrategyDTO
{
    public Instrument? Instrument { get; set; }
    public decimal CurrencyPnL { get; set; }
    public int Position { get; set; }
}

public static class OptionStrategyDtoExtension
{
    public static OptionStrategyDTO ToDto(this OptionStrategy strategy) => new OptionStrategyDTO
    {
        Instrument = strategy.Instrument,
        CurrencyPnL = strategy.GetCurrencyPnl(),
        Position = strategy.GetPosition()
    };
}