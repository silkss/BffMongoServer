﻿using System;
using Connectors;
using Common.Types.Base;
using Common.Types.Instruments;
using Strategies.Strategies.TradeUnions;

namespace Strategies.Builders.Helpers;

internal static class BuilderHelper
{
    public static Straddle CreateStraddle(
        IConnector connector, 
        Instrument parent, 
        double strike, 
        DateTime expiration)
    {
        connector
            .RequestPut(parent, strike, expiration, out Instrument? put)
            .RequestPut(parent, strike, expiration, out Instrument? call);
        if (put == null || call == null) throw new Exception("Cant request put or call");

        connector
            .RequestMarketData(call)
            .RequestMarketData(put);
        return new Straddle(call, put, Directions.Sell);
    }
}