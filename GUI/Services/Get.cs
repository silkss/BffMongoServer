using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Strategies.DTO;
using Connectors.Info;
using GUI.Services.Requests;

namespace GUI.Services;

internal static class Get
{

    public const string TRADER_ENDPOINT = "/api/trader/";
    public const string MCAPI_ENDPOINT = "/api/mcapi/";
    public const string STRATEGIES_ENDPOINT = "/api/containers/";
    public const string INSTRUMENT_ENDPOINT = "/api/instrument/";
    public const string CONNECTOR_ENDPOINT = "/api/connector/";
    
    public static readonly IEnumerable<string> Exchanges = new List<string>
    {
        "CME",
        "GLOBEX",
        "NYMEX"
    };

    public static ConnectorInfo? ConnectorInfo => ConnectorRequests.ConnectorInfo;
    public static ObservableCollection<MainStrategyDTO> AllSatrategies => StrategiesRequests.AllSatrategies;
    public static ObservableCollection<MainStrategyDTO> StrategiesInTrade => TradeRequests.StrategiesInTrade;

    public readonly static HttpClient Client = new HttpClient()
    {
#if DEBUG
        BaseAddress = new Uri("http://localhost:5001"),
#else
        BaseAddress = new Uri("http://192.168.0.3:5000"),
#endif
    };

    public readonly static TradeRequests TradeRequests = new TradeRequests(Client,TRADER_ENDPOINT, MCAPI_ENDPOINT);
    public readonly static ConnectorRequests ConnectorRequests = new ConnectorRequests(Client, CONNECTOR_ENDPOINT);
    public readonly static StrategiesRequests StrategiesRequests = new StrategiesRequests(Client, STRATEGIES_ENDPOINT);
    public readonly static InstrumentRequests InstrumentRequests = new InstrumentRequests(Client, INSTRUMENT_ENDPOINT);
    static Get()
    {
        Client.DefaultRequestHeaders.Accept.Clear();
        Client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }
}
