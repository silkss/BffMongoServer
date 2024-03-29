﻿namespace Strategies.Settings;

/// <summary>
/// Тут хранятся настройки спреда.
/// Спред это стратеги, в которой один опционной куплен, а второй продан.
/// В данный момент спред может быть Call и Put, тип спреда выбирается сигналом.
/// Call спред - страйк шортового Колла должен быть ВЫШЕ лонгового.
/// Put спред - страйк шортового Пута должен быть НИЖЕ лонгово.
/// </summary>
public class SpreadSettings
{
    /// <summary>
    /// Смещение покупного опциона относительно цены БАЗИСА.
    /// Смещенеи (сдвиг) указывает в шагах страйка.
    /// </summary>
    public int BuyStrikeShift { get; set; }

    /// <summary>
    /// Смещение продажного опциона относительно цены ПОКУПНОГО.
    /// Смещенеи (сдвиг) указывает в шагах страйка.
    /// </summary>
    public int SellStrikeShift { get; set; }
}
