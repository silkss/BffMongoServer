using Connectors.Info;
using IBApi;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace GUI.Services.Requests;

internal class ConnectorRequests : Base.Requests
{

    public ConnectorRequests(HttpClient client, string endpoint) : base(client, endpoint)
    { }

    public ConnectorInfo? ConnectorInfo { get; private set; }

    public async Task<ConnectorInfo?> GetConnectorInfo()
    {
        try
        {
            var resp = await _client.GetAsync(_endpoint);
            if (resp.IsSuccessStatusCode)
            {
                ConnectorInfo = await resp.Content.ReadAsAsync<ConnectorInfo>();
                
            }
        }
        catch(Exception)
        {
            Debug.WriteLine("Something wrong with requesting ConnectorInfo");
        }
        return ConnectorInfo;
    }
    public async Task<bool> ConnectAsync()
    {
        if (ConnectorInfo == null) return false;
        ConnectorInfo.IsConnected = true;
        var resp = await _client.PostAsJsonAsync(_endpoint, ConnectorInfo);
        if (resp.IsSuccessStatusCode)
        {
            ConnectorInfo = await resp.Content.ReadAsAsync<ConnectorInfo>();
            return true;
        }
        return false;
    }
    public async Task<bool> DisconnectAsync()
    {
        if (ConnectorInfo == null) return false;
        ConnectorInfo.IsConnected = false;
        var resp = await _client.PostAsJsonAsync(_endpoint, ConnectorInfo);
        if (resp.IsSuccessStatusCode)
        {
            ConnectorInfo = await resp.Content.ReadAsAsync<ConnectorInfo>();
            return true;
        }
        return false;
    }
}
