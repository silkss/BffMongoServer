using System.Net.Http;

namespace GUI.Services.Requests.Base;

internal class Requests
{
	protected readonly HttpClient _client;
    protected readonly string _endpoint;

	public Requests(HttpClient client, string endpoint)
	{
		_client = client;
		_endpoint = endpoint;
	}
}
