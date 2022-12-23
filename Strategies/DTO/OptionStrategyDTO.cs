using Common.Types.Base;
using Common.Types.Orders;
using Common.Types.Instruments;
using System.Collections.Generic;

namespace Strategies.DTO;

public class OptionStrategyDTO
{
    public Instrument? Instrument { get; set; }
    public decimal CurrencyPnL { get; set; }
    public decimal OpenPrice { get; set; }
    public int Position { get; set; }
    public TradeLogic Logic { get; set; }
    public List<Order>? Orders { get; set; }
}

public static class OptionStrategyDtoExtension
{
    public static OptionStrategyDTO ToDto(this Strategies.Depend.OptionStrategy strategy) => new OptionStrategyDTO
    {
        CurrencyPnL = strategy.GetCurrencyPnl(),
        Position = strategy.GetPosition(),
        Instrument = strategy.Instrument,
        OpenPrice = strategy.OpenPrice,
        Orders = strategy.Orders,
        Logic = strategy.Logic,
    };
}