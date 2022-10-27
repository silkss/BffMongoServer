using Strategies.Settings;

namespace Strategies.DTO;

public class MainStrategyDTO
{
    public MainSettings? MainSettings { get; set; }
    public StraddleSettings? StraddleSettings { get; set; }
    public ClosureSettings? ClosureSettings { get; set; }
}
