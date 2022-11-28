using Connectors;
using Connectors.Ib;
using MongoDbSettings;
using Notifier;
using Notifier.Implemintations;
using Strategies;
using System.Collections.ObjectModel;

namespace GuiStandAlone.Services;

internal static class Get
{
    private static StrategyDatabaseSettings _sds = new StrategyDatabaseSettings
    {
      ConnectionString = "mongodb://localhost:27017",
      DatabaseName = "StrategiesDev",
      CollectionName= "Strategies"
    };
    public static IBffLogger Logger { get; } = new DebugLogger();
    public static IConnector Connector { get; } = new IbConnector(Logger);
    public static StrategyService StrategyService { get; } = new StrategyService(_sds);
}
