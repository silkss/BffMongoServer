// See https://aka.ms/new-console-template for more information
using ContainerStore.Connectors.Ib;
using ContainerStore.Cui;
using ContainerStore.Data.Models.Instruments;
using System;


Console.WriteLine("Hello, World!");

var connector = new IbConnector(new CuiLogger<IbConnector>());
connector.Connect();
Instrument? _instrument = null;

Instrument? euro = null;
Instrument? gas = null;
Instrument? japan = null;

while (true)
{
    var cmd = Console.ReadLine()!.Trim().ToLower();

    if (cmd == "exit") break;
    if (cmd == "instrument")
    {
        euro = connector.RequestInstrument("NGZ2", "NYMEX");
        gas = connector.RequestInstrument("6EZ2", "GLOBEX");
        japan = connector.RequestInstrument("6JZ2", "GLOBEX");

        connector.RequestMarketData(euro);
        connector.RequestMarketData(gas);
        connector.RequestMarketData(japan);
        continue;
    }
    if (cmd == "price")
    {
        continue;
    }
    if (cmd == "opt")
    {
        var euroOpt = connector.GetOptionTradingClass(euro.Id, DateTime.Now.AddDays(30));
        var gasOpt = connector.GetOptionTradingClass(gas.Id, DateTime.Now.AddDays(30));
        var japanOpt = connector.GetOptionTradingClass(japan.Id, DateTime.Now.AddDays(20));

        var euroCall = connector.RequestCall(euro, euroOpt.Strikes[10], euroOpt.ExpirationDate);
        var gasCall = connector.RequestCall(gas, gasOpt.Strikes[10], gasOpt.ExpirationDate);
        var japanCall = connector.RequestCall(japan, japanOpt.Strikes[10], japanOpt.ExpirationDate);

        continue;
    }
    if (cmd == "rule")
    {
        connector.ReqMarketRule(98);
        connector.ReqMarketRule(239);
        connector.ReqMarketRule(776);
        connector.ReqMarketRule(98);
        connector.ReqMarketRule(580);
        connector.ReqMarketRule(922);
        continue;
    }
    if (cmd == "option chain")
    {
        if (_instrument == null) continue;
        connector.RequestOptionChain(_instrument);
        continue;
    }
}
