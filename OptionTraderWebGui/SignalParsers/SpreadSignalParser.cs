namespace OptionTraderWebGui.SignalParsers;

using Connectors;
using Traders.Builders;
using Traders.Strategies.BatmanStrategy;

internal class SpreadSignalParser
{
    private static (bool, string) openSpread(int direction, double price, BatmanContainer container, IConnector connector)
        => direction switch
        {
            -1 => BatmanBuilder.CreateAndAddContainer(container, price, connector),
            1 => BatmanBuilder.CreateAndAddContainer(container, price, connector),
            _ => (false, "Not implemented")
        };

    ///// <summary>
    ///// если <paramref name="direction"/> == Short (-1).
    ///// Необходимо купить пут, со страйком близким к цене(price) и продать пут со страйком НИЖЕ цены. 
    ///// Насколько ниже должно быть указано в настройках.
    ///// если <paramref name="direction"/> == Flat (0) - закрываем всё. Если все хорошо, конечноже.
    ///// если <paramref name="direction"/> == Long (1) - необходимо купить CALL,
    ///// со страйком близким к цене(price) и продать call со страйком ВЫШЕ цену.
    ///// Наскольколь выше должно быть в настройках.
    ///// </summary>
    ///// <param name="direction">1 - лонг. 0 - флэт. -1 - шорт.</param>
    ///// <param name="price">Цена на которой произошел вход - выход.(для выхода 0)</param>
    ///// <param name="container">Контейнер с настройками, в котором будем создавать спрэд.</param>
    ///// <param name="connector">Коннектор, который будет нам создавать инструменты.</param>
    ///// <param name="logger"></param>
    ///// <returns>Возвращает текст ошибки, если ошибка есть.</returns>
    //public static string ParseSignal(int direction, double price,
    //    Container container, IConnector connector) => "Not implemented";
    //    //container.GetOpenStrategyStatus() switch
    //    //{
    //    //    OptionStrategyStatus.NotExist => openSpread(direction, price, container, connector),
    //    //    OptionStrategyStatus.Working => "Working",
    //    //    _ => "Неизвестный статус опционной стратегии."
    //    //};
}
