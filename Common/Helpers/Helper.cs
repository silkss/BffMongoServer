using Common.Types.Base;
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
    
}
