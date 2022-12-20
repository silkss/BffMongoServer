using Connectors;
using Connectors.Ib;
using Notifier;
using Notifier.Implemintations;
using Strategies.Strategies;
using System.Collections.ObjectModel;

namespace GreatOptionTrader.Services;

internal static class Get
{
    public static IBffLogger Logger = new ConsoleLogger();
    public static IConnector Connector = new IbConnector(Logger);
    public static ObservableCollection<MainStrategy> Strategies = new();
}
