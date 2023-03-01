namespace Traders.Strategies.Types;

public enum OptionStrategyStatus
{
    Working, // Все хорошо, еще может продолжать работать
    Expired, // Время жизни стрэддла истекло.
    NotOpen, // Не открыл позицию.
    UnClosuredProfitLevelReached,
    ClosuredProfitLevelReached,
    InProfit, // Страддл накопил необходимый ПиУ.
    NotExist // На всякий  случай, вдруг гдет
}
