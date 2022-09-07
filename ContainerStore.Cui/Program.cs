// See https://aka.ms/new-console-template for more information
using ContainerStore.Connectors.Ib;
using ContainerStore.Cui;
using ContainerStore.Data.Models;
using System;

Console.WriteLine("Hello, World!");

var connector = new IbConnector(new CuiLogger<IbConnector>());
connector.Connect();
Instrument? _instrument = null;
while (true)
{
    var cmd = Console.ReadLine()!.Trim().ToLower();

    if (cmd == "exit") break;
    if (cmd == "instrument")
    {
        if (connector.RequestInstrument("6EZ2", "GLOBEX") is Instrument sec)
        {
            Console.WriteLine($"{sec.FullName}\t{sec.LastTradeDate}");
            _instrument = sec;
            continue;
        }
    }
    if (cmd == "option chain")
    {
        if (_instrument == null) continue;
        connector.RequestOptionChain(_instrument);
        continue;
    }
}
