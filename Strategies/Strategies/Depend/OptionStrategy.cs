using Strategies.Enums;
using System.Diagnostics.Metrics;
using System.Linq.Expressions;
using Transactions;

namespace Strategies.Strategies.Depend;

public class OptionStrategy : Base.Strategy
{
    public int Volume { get; set; }
    public Logic Logic { get; set; }
    public Transaction? OpenOrder { get; private set; }

    public static OptionStrategy CreateStraddleLeg(Instrument instrument, int volume = 1) => new OptionStrategy
    {
        Instrument = instrument,
        Logic = Logic.Open,
        Volume = volume,
    };

}
