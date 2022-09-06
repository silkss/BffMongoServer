using ContainerStore.Common.Enums;
using System;

namespace ContainerStore.Connectors.Helpers;

internal static class Helper
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
}
