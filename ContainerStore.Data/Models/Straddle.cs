using System;

namespace ContainerStore.Data.Models;

public class Straddle
{
    public decimal Strike  { get; set; }
    public DateTime ExpirationDate { get; set; }
    public StraddleLeg? CallLeg {  get; set; }
    public StraddleLeg? PutLeg { get; set; }
}