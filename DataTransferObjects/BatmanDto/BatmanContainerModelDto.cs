namespace DataTransferObjects.BatmanDto;

using Traders.Strategies.BatmanStrategy;

public class BatmanContainerModelDto {
    public BatmanContainerModelDto(BatmanContainer container)
    {
        Id = container.Id;
        InTrade = container.InTrade;
        BasisLastPrice = container.Instrument?.Last;
        TotalCurrencyPnlWithCommission = container.GetTotalCurrencyPnlWithCommission();
    }
    public bool InTrade { get; }
    public decimal TotalCurrencyPnlWithCommission { get; }
    public decimal? BasisLastPrice { get; }
    public string? Id { get; }
}
