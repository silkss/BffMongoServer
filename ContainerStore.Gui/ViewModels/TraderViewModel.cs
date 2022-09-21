using ContainerStore.Data.Models;
using ContainerStore.Gui.Commands;
using ContainerStore.Gui.Services;
using ContainerStore.Gui.ViewModels.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;

namespace ContainerStore.Gui.ViewModels;

internal class TraderViewModel : ViewModel
{
	private readonly string _traderEndpoint;
	private readonly HttpClient _client;

	private async void requestTradeContainers()
	{
		try
		{
			
			var res = await _client.GetAsync(_traderEndpoint);

			if (!res.IsSuccessStatusCode) return;
			if (await res.Content.ReadAsAsync<List<Container>>() is List<Container> containers)
			{
				App.Current.Dispatcher.Invoke(() =>
				{
					if (ContainersInTrade.Count > 0)
						ContainersInTrade.Clear();
					foreach (var container in containers)
					{
						ContainersInTrade.Add(container);
					}
				});
			}
		}
		catch (HttpRequestException)
		{
			ErrorMessage = "Не получилось устновить соединение с сервером!";
		}
		finally
		{
			ErrorMessage = "OK";
        }
	}
	public TraderViewModel()
	{
		_traderEndpoint = AppServices.TRADER_ENDPOINT;
		_client = AppServices.Client;

		Refresh = new LambdaCommand(onRefresh);
		Stop = new LambdaCommand(onStop, canStop);

		requestTradeContainers();
	}
	#region Prop
	private string _errorMessage = "all is ok";
	public string ErrorMessage
	{
		get => _errorMessage;
		set => Set(ref _errorMessage, value);
	}

	public ObservableCollection<Container> ContainersInTrade { get; } = new();
    #endregion
    #region Command
    public LambdaCommand Refresh { get; }
	private void onRefresh(object? obj) 
	{
		requestTradeContainers();
	}

	public LambdaCommand Stop { get; }
	private void onStop(object? obj)
	{

	}
	private bool canStop(object? obj) => obj is Container;
    #endregion
}
