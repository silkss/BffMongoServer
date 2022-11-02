using System.Collections.Generic;

namespace Strategies.Settings.Straddle;

public class StraddleSettings
{
    public decimal StraddleTargetPnl { get; set; } = 300.00m;

    /// <summary>
    /// StraddleLiveDays -- сколько максимум дней может быть открыт опцион.
    /// Чтобы стрэддл закрылся ему необходимо выполнение одно из двух условий:
    /// 1. Должен набраться Pnl <see cref="TargetPnlForStraddles"/>
    /// 2. Должен прожить не меньше <see cref="StraddleLiveDays"/> дней
    /// </summary>
    public int StraddleLiveDays { get; set; } = 2;

    /// <summary>
    /// StraddleExpirationDays минимальное "растояние" до даты экспирации.
    /// Указывается для того, чтобы выбирать максималльно удачный, по дате экспирации, 
    /// инструмент (опционы) для стреддла.
    /// </summary>
    public int StraddleExpirationDays { get; set; } = 10;

    public List<ProfitLevel> ClosuredProfitLevels { get; set; } = new();
    public List<ProfitLevel> UnClosuredProfitLevels { get; set; } = new();
}
