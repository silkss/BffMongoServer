namespace GUI.Services.Requests;

using System.Net.Http;
using System.Threading.Tasks;

internal class StrategiesRequests : Base.Requests
{
    
    public StrategiesRequests(HttpClient client, string endpoint) : base(client, endpoint)
    { }

    public async void RefreshAsync()
    {

    }
    public async Task<bool> UpdateAsync(MainStrategyDTO strategy)
    {
        var resp = await _client.PutAsJsonAsync<MainStrategyDTO>(_endpoint + strategy.Id, strategy);
        if (resp.IsSuccessStatusCode)
        {
            return true;
        }
        return false;
    }
    public async Task<bool> RemoveAsync(string? id)
    {
        if (id == null) return false;

        var res = await _client.DeleteAsync(_endpoint + id);
        return res.IsSuccessStatusCode;
    }
    public async Task<bool> CreateAsync()
    {
        //var resp = await _client.PostAsJsonAsync(_endpoint, strategyDTO);
        //if (resp.IsSuccessStatusCode)
        //{
        //    return true;
        //}
        return false;
    }
}
