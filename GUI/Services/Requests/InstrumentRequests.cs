using Instruments;
using System.Net.Http;
using System.Threading.Tasks;

namespace GUI.Services.Requests;

internal class InstrumentRequests : Base.Requests
{
	public InstrumentRequests(HttpClient client, string endpoint) : base(client, endpoint)
	{ }
    public async Task<Instrument?> GetAsync(string localname, string exchange)
    {
        Instrument? requstedInstument = null;
        var response = await _client.GetAsync(_endpoint + $"?localname={localname}&exchange={exchange}");
        if (response.IsSuccessStatusCode)
        {
            requstedInstument = await response.Content.ReadAsAsync<Instrument>();
        }
        return requstedInstument;
    }
}
