using Connectors;
using Connectors.Ib;
using Notifier;
using Notifier.Implemintations;
using System;

internal class Program
{
    static IBffLogger _bffLogger = new ConsoleLogger();
    static IConnector _connector = new IbConnector(_bffLogger);
    private static void Main(string[] args)
    {
        _connector.Connect();
        Console.ReadKey();

        var sec = _connector.RequestInstrument("6EF3", "CME");
        if (sec != null)
        {
            Console.WriteLine($"{sec.FullName}\t{sec.LastTradeDate}");
            _connector.RequestMarketData(sec);
        }
        Console.ReadKey();
    }
}