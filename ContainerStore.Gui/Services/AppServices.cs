using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ContainerStore.Gui.Services;

internal static class AppServices
{
    public const string TRADER_ENDPOINT = "/api/trader/";
    public const string CONTAINERS_ENDPOINT = "/api/containers/";
    public const string CONNECTOR_ENDPOINT = "/api/connector/";
    public const string INSTRUMENT_ENDPOINT = "api/instrument/";

    public readonly static HttpClient Client;
	static AppServices()
	{
		Client = new HttpClient();
		Client.BaseAddress = new Uri("http://localhost:5001");
        Client.DefaultRequestHeaders.Accept.Clear();
        Client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }
}
