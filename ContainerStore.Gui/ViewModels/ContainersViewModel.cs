using ContainerStore.Data.Models;
using ContainerStore.Gui.Commands;
using ContainerStore.Gui.Services;
using ContainerStore.Gui.ViewModels.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;

namespace ContainerStore.Gui.ViewModels;

internal class ContainersViewModel : ViewModel
{
    private readonly string _containersEndpoint;
    private readonly string _traderEndpoint;
    private const string TRADER_PATH = "api/trader/";
    private readonly HttpClient _client;

    private async void requestContainers()
    {
        var res = await _client.GetAsync(_containersEndpoint);
        {
            if (!res.IsSuccessStatusCode) return;
            if (await res.Content.ReadAsAsync<List<Container>>() is List<Container> containers)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    if (Containers.Count > 0)
                        Containers.Clear();
                    foreach (var container in containers)
                    {
                        Containers.Add(container);
                    }
                });
            }
        }
    }
	public ContainersViewModel()
	{
        _containersEndpoint = AppServices.CONTAINER_ENDPOINT;
        _traderEndpoint = AppServices.TRADER_ENDPOINT;

		_client = AppServices.Client;

        Refresh = new LambdaCommand(onRefresh);
        Modify = new LambdaCommand(onModify, canModify);
        Delete = new LambdaCommand(onDelete, canDelete);
        AddToTrade = new LambdaCommand(onAddToTrade, canAddToTrade);

        requestContainers();
	}
    #region Props
    public ObservableCollection<Container> Containers { get; } = new();
    #endregion
    #region Commands
    public LambdaCommand Refresh { get; }
    private void onRefresh(object? obj)
    {   
        requestContainers();
    }

    public LambdaCommand Modify { get; }
    private void onModify(object? obj)
    {

    }
    private bool canModify(object? obj) => obj != null;

    public LambdaCommand Delete { get; }
    private void onDelete(object? obj)
    {

    }
    private bool canDelete(object? obj) => obj != null;

    public LambdaCommand AddToTrade { get; }
    private async void onAddToTrade(object? obj) 
    {
        if (obj == null) return;
        if (obj is Container container)
        {
            if (container.Id == null) return;

            var res = await _client.PostAsync(_traderEndpoint + container.Id, null);
            if (res.IsSuccessStatusCode)
            {

            }
        }
    }
    private bool canAddToTrade(object? obj) => obj != null;
    #endregion
}
