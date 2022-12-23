using Connectors;
using Notifier;
using Strategies.Strategies;

namespace WebApi.SignalParsers;

internal class SpreadSignalParser
{
    public static string ParseSignal(int direction, double price, 
        MainStrategy strategy, IConnector connector, IBffLogger logger)
    {
        return "parsing";
    }
}
