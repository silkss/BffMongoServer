namespace OptionTraderWebGui.SignalParsers;

using Connectors;
using Notifier;
using Strategies;
using Strategies.Containers;
using Strategies.Types;
using Traders.Builders;

internal class SpreadSignalParser
{
    private static string openSpread(int direction, double price, Container container, IConnector connector)
        => direction switch
        {
            -1 => SpreadBuilder.OpenSpread(container, price, connector,isLong:false),
            1 => SpreadBuilder.OpenSpread(container, price, connector,isLong:true),
            _ => "OK"
        };

    /// <summary>
    /// если <paramref name="direction"/> == Short (-1).
    /// Необходимо купить пут, со страйком близким к цене(price) и продать пут со страйком НИЖЕ цены. 
    /// Насколько ниже должно быть указано в настройках.
    /// если <paramref name="direction"/> == Flat (0) - закрываем всё. Если все хорошо, конечноже.
    /// если <paramref name="direction"/> == Long (1) - необходимо купить CALL,
    /// со страйком близким к цене(price) и продать call со страйком ВЫШЕ цену.
    /// Наскольколь выше должно быть в настройках.
    /// </summary>
    /// <param name="direction">1 - лонг. 0 - флэт. -1 - шорт.</param>
    /// <param name="price">Цена на которой произошел вход - выход.(для выхода 0)</param>
    /// <param name="container">Контейнер с настройками, в котором будем создавать спрэд.</param>
    /// <param name="connector">Коннектор, который будет нам создавать инструменты.</param>
    /// <param name="logger"></param>
    /// <returns>Возвращает текст ошибки, если ошибка есть.</returns>
    public static string ParseSignal(int direction, double price,
        Container container, IConnector connector, IBffLogger logger) => 
        container.GetOpenStrategyStatus() switch
        {
            OptionStrategyStatus.NotExist => openSpread(direction, price, container, connector),
            OptionStrategyStatus.Working => "Working",
            _ => "Неизвестный статус опционной стратегии."
        };

    public static string ParseSpreadWithHedgeSpreadSignal(int direction, double price,
        SpreadWithHedgeSpreadContainer container, IConnector connector, IBffLogger logger)
    {
        if (container.IsWorking()) return "Container working";
        if (direction == 1)
            return container.CreateSpread(connector, price, isLong: true);
        else
            return "Not implemented";
    }
}
