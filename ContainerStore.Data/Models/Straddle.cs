using ContainerStore.Common.Enums;
using ContainerStore.Data.Models.TradeUnits;
using Instruments;
using MongoDB.Bson.Serialization.Attributes;
using Strategies.Enums;
using System;

namespace ContainerStore.Data.Models;

public class Straddle
{
    private StraddleLeg createLeg(Instrument instrument) => new StraddleLeg
    {
        Instrument = instrument,
        Logic  = Logic.Open
    }\
    public Straddle() { }
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
    public Logic Logic { get; set; }
    public StraddleLeg CallLeg {  get; set; }
    public StraddleLeg PutLeg { get; set; }
    public DateTime CreatedTime { get; set; }
    public void Stop()
    {
        CallLeg?.Stop();
        PutLeg?.Stop();
    }
    public void Close()
    {
        Logic = TradeLogic.Close;
        CallLeg?.Close();
        PutLeg?.Close();
    }
    public bool IsDone() => CallLeg.IsDone() || PutLeg.IsDone();
    [BsonIgnore]
    public decimal CurrencyPnl
    {
        get
        {
            var currency_pnl = 0m;
            currency_pnl = CallLeg.CurrencyPnl + PutLeg.CurrencyPnl;
            if (CallLeg.Closure != null)
            {
                currency_pnl += CallLeg.Closure.CurrencyPnl;
            }
            if (PutLeg.Closure != null)
            {
                currency_pnl += PutLeg.Closure.CurrencyPnl;
            }

            return currency_pnl;
        }
    }
    public decimal GetPnl()
    {
        var pnl = 0m;

        pnl = CallLeg.GetPnl() + PutLeg.GetPnl();
        if (CallLeg.Closure != null)
        {
            pnl += CallLeg.Closure.GetPnl();
        }
        if (PutLeg.Closure != null)
        {
            pnl += PutLeg.Closure.GetPnl();
        }
        return pnl;
    }
}