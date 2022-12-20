using Connectors;
using Connectors.Ib;
using GreatOptionTraderStrategies.Strategies.Base;
using Notifier;
using Notifier.Implemintations;
using System.Collections.ObjectModel;

namespace GreatOptionTrader.Services;

internal static class Get
{
    public static IBffLogger Logger = new ConsoleLogger();
    public static IConnector Connector = new IbConnector(Logger);
    public static ObservableCollection<Container> Containers = new();
}
