using ContainerStore.Common.Enums;
using ContainerStore.Data.Models.TradeUnits;
using System;

namespace ContainerStore.Data.Models;

public class Straddle
{
    public decimal Strike  { get; set; }
    public DateTime ExpirationDate { get; set; }
    public TradeLogic Logic { get; set; }
    public StraddleLeg CallLeg {  get; set; }
    public StraddleLeg PutLeg { get; set; }
    public DateTime CreatedTime { get; set; }
    public Straddle(Instrument call, Instrument put)
    {
        CallLeg = new StraddleLeg
        {
            Instrument = call,
            Logic = TradeLogic.Open,
        };
        PutLeg = new StraddleLeg
        {
            Instrument = put,
            Logic = TradeLogic.Open,
        };

        ExpirationDate = call.LastTradeDate;
        CreatedTime = DateTime.UtcNow;
        Strike = call.Strike;
    }
    public void Stop()
    {
        CallLeg?.Stop();
        PutLeg?.Stop();
    }
}