using Instruments;

namespace Strategies.DTO;

public class MainStrategyDTO : MainStrategySettingsDTO
{
    public string? Id { get; set; }
    public Instrument? Instrument { get; set; }
    public decimal Pnl { get; set; }
    public static MainStrategyDTO GetFrom(MainStrategy strategy) => new MainStrategyDTO
    {
        Id = strategy.Id,
        Instrument = strategy.Instrument,
        Pnl = strategy.GetAllPnl()
    };
}
