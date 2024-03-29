﻿using Common.Types.Base;
using System;

namespace Common.Helpers;

public static class Helper
{
    public static InstrumentType IbSecTypeToBffEnum(string sectype) => sectype switch
    {
        "FUT" => InstrumentType.Future,
        "FOP" => InstrumentType.Option,
        _ => InstrumentType.Future
    };
    public static decimal ConvertDoubleToDecimal(double value)
    {
        var newValue = 0m;
        if (value == double.MaxValue) return newValue;
        try
        {
            newValue = (decimal)value;
        }
        catch (OverflowException)
        {
            newValue = 0;
        }

        return newValue;
    }
    public static decimal RoundUp(decimal value, decimal step)
    {
        if (step == 0)
        {
            return 0;
        }

        var multiplicand = Math.Round(value / step);
        return multiplicand * step;
    }
}
