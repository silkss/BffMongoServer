using ContainerStore.Data.ServiceModel;
using ContainerStore.Gui.Commands;
using ContainerStore.Gui.Services;
using ContainerStore.Gui.ViewModels.Base;
using System.Net.Http;

namespace ContainerStore.Gui.ViewModels;

internal class ConnectorViewModel : ViewModel
{
    private const string PATH = "api/connector";
	private readonly HttpClient _client;

	private void setProperties(ConnectorModel? model)
	{
		if (model == null)
		{
			Host = "NO DATA!";
			return;
		}
        Host = model.Host;
        Port = model.Port;
        ClientId = model.ClientId;
        IsConnected = model.IsConnected;
    }
	public ConnectorViewModel()
	{
        _client = AppContext.Client;
		HttpResponseMessage response = _client.GetAsync(PATH).Result;
		if (response.IsSuccessStatusCode)
		{
			setProperties(response.Content.ReadAsAsync<ConnectorModel>().Result);
		}

		Connect = new LambdaCommand(onConnect);
		Disconnect = new LambdaCommand(onDisconnect);
	}
    #region Props

    private string _host;
	public string Host
	{
		get => _host;
		set => Set(ref _host, value);
	}

	private int _port;
	public int Port
	{
		get => _port;
		set => Set(ref _port, value);
	}

	private int _clientId;
	public int ClientId
	{
		get => _clientId;
		set => Set(ref _clientId, value);
	}

	private bool _isConnected;
	public bool IsConnected
	{
		get => _isConnected;
		set => Set(ref _isConnected, value);
	}
    #endregion
    #region Commands
	public LambdaCommand Connect { get; }
	private async void onConnect(object? obj)
	{
        var model = new ConnectorModel
        {
            Host = Host,
            Port = Port,
            ClientId = ClientId,
            IsConnected = true,
        };

        var client = AppContext.Client;
        var res = await client.PostAsJsonAsync(PATH, model);

        if (res.IsSuccessStatusCode)
        {
            model = await res.Content.ReadAsAsync<ConnectorModel>();
            setProperties(model);
        }
    }
	public LambdaCommand Disconnect { get; }
	private async void onDisconnect(object? obj)
	{
        var model = new ConnectorModel
        {
            Host = Host,
            Port = Port,
            ClientId = ClientId,
            IsConnected = false,
        };

        var res = await _client.PostAsJsonAsync(PATH, model);

        if (res.IsSuccessStatusCode)
        {
            model = await res.Content.ReadAsAsync<ConnectorModel>();
            setProperties(model);
        }
    }
    #endregion
}
