using ContainerStore.Data.Models;
using ContainerStore.Gui.Commands;
using ContainerStore.Gui.Services;
using ContainerStore.Gui.ViewModels.Base;
using System.Net.Http;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ContainerStore.Gui.ViewModels;

internal class ContainersViewModel : ViewModel
{
    private readonly string _containersEndpoint;
    private readonly string _traderEndpoint;
    private readonly HttpClient _client;

    private async void requestContainers()
    {
        try
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
        catch (HttpRequestException)
        {
            ErrorMessage = "Не получилось устновить соединение с сервером!";
        }
        finally
        {
            ErrorMessage = "Ok";
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
    private string _errorMessage = "All is ok";
    public string ErrorMessage
    {
        get => _errorMessage;
        set => Set(ref _errorMessage, value);
    }
    public ObservableCollection<Container> Containers { get; } = new();
    #endregion
    #region Commands
    #region Refresh
    public LambdaCommand Refresh { get; }
    private void onRefresh(object? obj)
    {   
        requestContainers();
    }
    #endregion
    #region Modify
    public LambdaCommand Modify { get; }
    private void onModify(object? obj)
    {

    }
    private bool canModify(object? obj) => obj != null;
    #endregion
    #region Delete
    public LambdaCommand Delete { get; }
    private async void onDelete(object? obj)
    {
        if (obj is Container container)
        {
            if (container.Id == null) return;
            try
            {
                var res = await _client.DeleteAsync(_containersEndpoint + container.Id);
                if (res.IsSuccessStatusCode)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Containers.Remove(container);
                    });
                }
            }
            catch (HttpRequestException)
            {
                ErrorMessage = "Не получилось устновить соединение с сервером!";
            }
        }
    }
    private bool canDelete(object? obj) => obj is Container;
    #endregion
    #region AddToTrade
    public LambdaCommand AddToTrade { get; }
    private async void onAddToTrade(object? obj) 
    {
        if (obj == null) return;
        if (obj is Container container)
        {
            if (container.Id == null) return;
            await _client.PostAsync(_traderEndpoint + container.Id, null);
        }
    }
    private bool canAddToTrade(object? obj) => obj is Container;
    #endregion
    #endregion
}
