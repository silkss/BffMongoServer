using ContainerStore.Common.Enums;
using ContainerStore.Data.Models.Instruments;
using ContainerStore.Data.Models.TradeUnits;
using System;

namespace ContainerStore.Data.Models;

public class Straddle
{
    private StraddleLeg createLeg(Instrument instrument) => new StraddleLeg
    {
        Instrument = instrument,
        Logic  = TradeLogic.Open
    };
    
    public Straddle(Instrument call, Instrument put)
    {
        CallLeg = createLeg(call);
        PutLeg = createLeg(put);
        ExpirationDate = call.LastTradeDate;
        CreatedTime = DateTime.UtcNow;
        Strike = call.Strike;
    }
    public Straddle(Instrument call, Instrument closureCall, Instrument put, Instrument closurePut)
    {
        CallLeg = createLeg(call).AddClosure(closureCall);
        PutLeg = createLeg(put).AddClosure(closurePut);
        ExpirationDate = call.LastTradeDate;
        CreatedTime = DateTime.UtcNow;
        Strike = call.Strike;
    }
    public decimal Strike  { get; set; }
    public DateTime ExpirationDate { get; set; }
    public TradeLogic Logic { get; set; }
    public StraddleLeg CallLeg {  get; set; }
    public StraddleLeg PutLeg { get; set; }
    public DateTime CreatedTime { get; set; }
    public void Stop()
    {
        CallLeg?.Stop();
        PutLeg?.Stop();
    }
}