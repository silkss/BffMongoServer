using System.Net.Http.Headers;
using System.Net.Http;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;
using Strategies.DTO;
using Instruments;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ContainerStore.Connectors.Info;
using IBApi;
using System.ComponentModel;
using System.Security.Policy;

namespace GUI.Services;

internal static class Get
{
    public const string TRADER_ENDPOINT = "/api/trader/";
    public const string MCAPI_ENDPOINT = "/api/mcapi/";
    public const string CONTAINERS_ENDPOINT = "/api/containers/";
    public const string INSTRUMENT_ENDPOINT = "/api/instrument/";
    public const string CONNECTOR_ENDPOINT = "/api/connector/";
    

    public static readonly IEnumerable<string> Exchanges = new List<string>
    {
        "CME",
        "GLOBEX",
        "NYMEX"
    };

    public static async void RequestAllStrategies()
    {
        try
        {
            var response = await Client.GetAsync(CONTAINERS_ENDPOINT);
            if (response.IsSuccessStatusCode)
            {
                var strategies = await response.Content.ReadAsAsync<List<MainStrategyDTO>>();
                App.Current.Dispatcher.Invoke(() =>
                {
                    AllSatrategies.Clear();
                    foreach (var strategy in strategies)
                    {
                        AllSatrategies.Add(strategy);
                    }
                });
            }
        }
        catch (HttpRequestException)
        {
            Debug.WriteLine("Не удалось выполнить запрос");
        }
    }
    public static async Task<bool> RequestModifyStrategy(MainStrategyDTO strategy)
    {
        var resp = await Client.PutAsJsonAsync<MainStrategyDTO>(CONTAINERS_ENDPOINT+strategy.Id, strategy);
        if (resp.IsSuccessStatusCode)
        {
            return true;
        }
        return false;
    }

    public static async void RequestStrategiesInTrade()
    {
        try
        {
            var resp = await Client.GetAsync(TRADER_ENDPOINT);
            if (resp.IsSuccessStatusCode)
            {
                var strategies = await resp.Content.ReadAsAsync<List<MainStrategyDTO>>();
                App.Current.Dispatcher.Invoke(() =>
                {
                    StrategiesInTrade.Clear();
                    foreach (var strategy in strategies)
                    {
                        StrategiesInTrade.Add(strategy);
                    }
                });
            }
        }
        catch (HttpRequestException)
        {
            Debug.WriteLine("Не удалось выполнить запрос");
        }
    }

    public static async Task<Instrument?> InstrumentAsync(string localname, string exchange)
    {
        Instrument? requstedInstument = null;
        var response = await Client.GetAsync(INSTRUMENT_ENDPOINT + $"?localname={localname}&exchange={exchange}");
        if (response.IsSuccessStatusCode)
        {
            requstedInstument = await response.Content.ReadAsAsync<Instrument>();
        }
        return requstedInstument;
    }
    public static async Task<bool> CreateStrategyAsync(MainStrategyDTO strategyDTO)
    {
        var resp = await Client.PostAsJsonAsync(CONTAINERS_ENDPOINT, strategyDTO);
        if (resp.IsSuccessStatusCode)
        {
            return true;
        }
        return false;
    }
    public static async Task<bool> RequesDeleteStrategy(string? id)
    {
        if (id == null) return false;

        var res = await Client.DeleteAsync(CONTAINERS_ENDPOINT + id);
        return res.IsSuccessStatusCode;
    }

    public static async Task<bool> RequestConnectAsync()
    {
        if (ConnectorInfo == null) return false;
        ConnectorInfo.IsConnected = true;
        var resp = await Client.PostAsJsonAsync(CONNECTOR_ENDPOINT, ConnectorInfo);
        if (resp.IsSuccessStatusCode)
        {
            ConnectorInfo = await resp.Content.ReadAsAsync<ConnectorInfo>();
            return true;
        }
        return false;
    }
    public static async Task<ConnectorInfo?> RequestConnectorInfo()
    {
        var resp = await Client.GetAsync(CONNECTOR_ENDPOINT);
        if (resp.IsSuccessStatusCode)
        {
            ConnectorInfo = await resp.Content.ReadAsAsync<ConnectorInfo>();
            return ConnectorInfo;
        }
        return ConnectorInfo;
    }
    public static async Task<bool> RequestDisconnectAsync()
    {
        if (ConnectorInfo == null) return false;
        ConnectorInfo.IsConnected = false;
        var resp = await Client.PostAsJsonAsync(CONNECTOR_ENDPOINT, ConnectorInfo);
        if (resp.IsSuccessStatusCode)
        {
            ConnectorInfo = await resp.Content.ReadAsAsync<ConnectorInfo>();
            return true;
        }
        return false;
    }

    public static async Task<bool> RequestAddToTrader(string? id)
    {
        if (id == null) return false;
        var res = await Client.PostAsync(TRADER_ENDPOINT + id, null);
        Debug.WriteLine(res.StatusCode);
        return res.IsSuccessStatusCode;
    }
    public static async Task<bool> RequestRemoveFromTraade(string? id)
    {
        if (id == null) return false;
        var res = await Client.DeleteAsync(TRADER_ENDPOINT + id);
        Debug.WriteLine(res.StatusCode);
        return res.IsSuccessStatusCode;
    }
    
    public static ConnectorInfo? ConnectorInfo { get; private set; } 
    public readonly static ObservableCollection<MainStrategyDTO> AllSatrategies = new();
    public readonly static ObservableCollection<MainStrategyDTO> StrategiesInTrade  = new();
    public readonly static HttpClient Client = new HttpClient()
    {
#if DEBUG
        BaseAddress = new Uri("http://localhost:5001"),
#else
        BaseAddress = new Uri("http://192.168.0.3:5000"),
#endif
    };

    static Get()
    {
        Client.DefaultRequestHeaders.Accept.Clear();
        Client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }
}
