using Instruments;

namespace Strategies.DTO;

public class MainStrategyDTO : MainStrategySettingsDTO
{
    public string? Id { get; set; }
    public Instrument? Instrument { get; set; }
    public decimal PnlCurrency { get; set; }
    public decimal? OpenPnlCurrency { get; set; }
    public StraddleDTO? OpenStraddle { get; set; }

    public static MainStrategyDTO GetFrom(MainStrategy strategy) => new MainStrategyDTO
    {
        Id = strategy.Id,
        Instrument = strategy.Instrument,
        PnlCurrency = strategy.GetAllPnlCurrency(),
        MainSettings = strategy.MainSettings,
        ClosureSettings = strategy.ClosureSettings,
        StraddleSettings = strategy.StraddleSettings,
        OpenPnlCurrency = strategy.GetOpenPnlCurrency(),
        OpenStraddle = strategy.GetOpenStraddle()?.ToDto(),
    };
}
