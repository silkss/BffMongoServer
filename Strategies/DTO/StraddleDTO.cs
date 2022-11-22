using Common.Enums;
using Strategies.Depend;
using Strategies.TradeUnions;
using System;
using System.Linq;

namespace Strategies.DTO;

public class StraddleDTO
{
    public OptionStrategyDTO? Call { get; set; }
    public OptionStrategyDTO? Put { get; set; }
    public OptionStrategyDTO? CallClosure { get; set; }
    public OptionStrategyDTO? PutClosure { get; set; }
    public DateTime CreatedDate { get; set; }
    public TimeSpan DaysAfterOpening { get; set; }
}
public static class StraddleExtensions
{
    private static OptionStrategy? get(OptionType type, Straddle straddle) => straddle.Legs
        .FirstOrDefault(l => l.Instrument.OptionType == type);
    private static OptionStrategy? getClosure(OptionType type, Straddle straddle) => get(type, straddle)?.Closure;

    public static StraddleDTO? ToDto(this Straddle? straddle) => straddle == null ?
        null :
        new StraddleDTO
        {
            CreatedDate = straddle.CreatedTime,
            DaysAfterOpening = straddle.GetDaysAfterOpening(),
            Call = get(OptionType.Call, straddle)?.ToDto(),
            CallClosure = getClosure(OptionType.Call, straddle)?.ToDto(),
            Put = get(OptionType.Put, straddle)?.ToDto(),
            PutClosure = getClosure(OptionType.Put, straddle)?.ToDto(),
        };
}
