using Strategies.Settings;
using Strategies.Settings.Straddle;

namespace Strategies.DTO;

public class MainStrategySettingsDTO
{
    public MainSettings? MainSettings { get; set; }
    public ClosureSettings? ClosureSettings { get; set; }
    public StraddleSettings? StraddleSettings { get; set; }
}
