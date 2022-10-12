namespace ContainerStore.Common.DTO;

public class ContainerDTO
{
    public string Account { get; set; }
    public int StraddleLiveDays { get; set; }
    public int StraddleExpirationDays { get; set; }
    public int ClosureStrikeStep { get; set; }
    public int ClosurePriceGapProcent { get; set; }
    public int OrderPriceShift { get; set; } = 2;
}
