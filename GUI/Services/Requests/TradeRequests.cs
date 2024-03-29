﻿using IBApi;
using Strategies.DTO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace GUI.Services.Requests;

internal class TradeRequests : Base.Requests
{
    private readonly string _mcEndpoint;

    public TradeRequests(HttpClient client, string endpoint, string mcEndpoint) : base(client, endpoint)
	{
        _mcEndpoint = mcEndpoint;
    }
    public async Task<bool> AlertStopAsync()
    {
        if (strategy.Instrument == null && strategy.MainSettings == null)
        {
            return false;
        }

        var query = HttpUtility.ParseQueryString(string.Empty);
        //query["symbol"] = strategy.Instrument?.FullName;
        //query["price"] = "0";
        //query["account"] = strategy.MainSettings?.Account;
        //query["type"] = "ALARMCLOSE";

        if (query.ToString() is string querystring)
        {
            var res = await _client.GetAsync(_mcEndpoint + "?" + querystring);
            if (res.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }
    public async Task<bool> StartStrategy(string? strategyId)
    {
        if (strategyId == null) return false;
        var res = await _client.PostAsync(_endpoint + strategyId, null);
        Debug.WriteLine(res.StatusCode);
        return res.IsSuccessStatusCode;
    }
    public async Task<MainStrategyDTO?> GetStrategy(string? strategyId)
    {
        if (strategyId == null) return null;

        MainStrategyDTO? strategy = null;
        try
        {
            var resp = await _client.GetAsync(_endpoint + strategyId);

            if (resp.IsSuccessStatusCode)
            {
                strategy = await resp.Content.ReadAsAsync<MainStrategyDTO>();
            }
        }
        catch (Exception)
        {
            Debug.WriteLine("Something wrong with requesting details");
        }
        return strategy;
    }
    public async Task<bool> StopTrategy(string? strategyId)
    {
        if (strategyId == null) return false;
        var res = await _client.DeleteAsync(_endpoint + strategyId);
        Debug.WriteLine(res.StatusCode);
        return res.IsSuccessStatusCode;
    }
    public async void RefreshAsync()
    {
        try
        {
            var resp = await _client.GetAsync(_endpoint);
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
}
