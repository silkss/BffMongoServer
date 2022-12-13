namespace Strategies.Settings;

public class ClosureSettings
{
    public int ClosureStrikeStep { get; set; } = 5;

    /// <summary>
    /// Относительная величина (в проценитах) показывающая по какой цене необходимо продать Замыкающий котракт
    /// относительно главной ноги стрэддла. (по умолчанию 100 процентов. т.е. замыкающий контракт должен продасться 
    /// по той же цене, по которой куплена соответсвующая нога стрэддла.
    /// </summary>
    public int ClosurePriceGapProcent { get; set; } = 100;

    /// <summary>
    /// Величина (в процентах) на которую должна изменится цена базиса (относительно центрального страйка)
    /// для срабатывания Closure по текущей теор цене (Замыкающая стратегия продается по текущей цене, игнорируя 
    /// <see cref="ClosurePriceGapProcent"/>)
    /// </summary>
    public decimal ClosureTrigerProcent { get; set; } = 1;
}
