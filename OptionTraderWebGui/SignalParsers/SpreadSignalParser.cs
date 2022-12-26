using Connectors;
using Microsoft.Net.Http.Headers;
using Notifier;
using Strategies;
using Strategies.Types;
using Traders.Builders;

namespace OptionTraderWebGui.SignalParsers;

internal class SpreadSignalParser
{
    private static string openShortSpread(double price, Container container, IConnector connector)
    {
        return "1";
    }
    private static string openSpread(int direction, double price, Container container, IConnector connector)
        => direction switch
        {
            -1 => SpreadBuilder.OpenShortSpread(container, price, connector),
            _ => "OK"
        };
    //    switch (direction)
    //    {
    //        //Шорт. Необходимо купить пут, со страйком близким к цене(price)
    //        //продать пут со страйком НИЖЕ цены. Насколько ниже должно быть указано в настройках.
    //        case -1: 
    //            break;
    //        //FLAT - закрываем всё. Если все хорошо, конечноже.
    //        case 0:
    //            break;
    //        //ЛОНГ - необходимо купить CALL, со страйком близким к цене(price)
    //        //продать call со страйком ВЫШЕ цену. Наскольколь выше должно быть в настройках.
    //        case 1:
    //            break;
    //        default:
    //            break;
    //    }
    //    return "Opened";
    //}
    public static string ParseSignal(int direction, double price,
        Container container, IConnector connector, IBffLogger logger) => 
        container.GetOpenStrategyStatus() switch
        {
            OptionStrategyStatus.NotExist => openSpread(direction, price, container, connector),
            OptionStrategyStatus.Working => "Working",
            _ => "something what I dont know yet."
        };
}
