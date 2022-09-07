using ContainerStore.Common.Enums;
using System;

namespace ContainerStore.Data.Models;

public class Straddle
{
    public decimal Strike  { get; set; }
    public DateTime ExpirationDate { get; set; }
    public TradeLogic Logic { get; set; }
    public StraddleLeg? CallLeg {  get; set; }
    public StraddleLeg? PutLeg { get; set; }
}