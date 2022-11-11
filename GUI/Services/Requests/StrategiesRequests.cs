using IBApi;
using Strategies.DTO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace GUI.Services.Requests;

internal class StrategiesRequests : Base.Requests
{
    
    public StrategiesRequests(HttpClient client, string endpoint) : base(client, endpoint)
    { }
    public ObservableCollection<MainStrategyDTO> AllSatrategies { get; } = new();

    public async void RefreshAsync()
    {
        try
        {
            var response = await _client.GetAsync(_endpoint);
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
    public async Task<bool> CreateAsync(MainStrategyDTO strategyDTO)
    {
        var resp = await _client.PostAsJsonAsync(_endpoint, strategyDTO);
        if (resp.IsSuccessStatusCode)
        {
            return true;
        }
        return false;
    }
}
