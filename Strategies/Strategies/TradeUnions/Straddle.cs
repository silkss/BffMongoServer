using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using Strategies.Strategies.Depend;

namespace Strategies.Strategies.TradeUnions;

public class Straddle
{
	public Straddle(Instrument call, Instrument put)
	{
		Legs[0] = OptionStrategy.CreateStraddleLeg(call, volume: 1);
		Legs[1] = OptionStrategy.CreateStraddleLeg(put, volume: 1);
	}

    public List<OptionStrategy> Legs { get; } = new List<OptionStrategy>(2);
	public DateTime CreatedTime { get; set; }
}
