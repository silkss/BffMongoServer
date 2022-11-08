﻿namespace Strategies.Enums;

public enum StraddleStatus
{
    Working, // Все хорошо, еще может продолжать работать
    Expired, // Время жизни стрэддла истекло.
    NotOpen, // Не открыл позицию.
    UnClosureProfitLevelReached,
    InProfit, // Страддл накопил необходимый ПиУ.
    NotExist // На всякий  случай, вдруг гдет
}
