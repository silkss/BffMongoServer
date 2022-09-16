using ContainerStore.Data.ServiceModel;
using ContainerStore.Gui.Commands;
using ContainerStore.Gui.Services;
using ContainerStore.Gui.ViewModels.Base;
using System.Net.Http;

namespace ContainerStore.Gui.ViewModels;

internal class ConnectorViewModel : ViewModel
{
	private readonly string _connectorEndpoint;
	private readonly HttpClient _client;

	private async void reqAccountInfo()
	{
        HttpResponseMessage response = await _client.GetAsync(_connectorEndpoint);
        if (response.IsSuccessStatusCode)
        {
            setProperties(await response.Content.ReadAsAsync<ConnectorModel>());
        }
    }
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
		_connectorEndpoint = AppServices.CONNECTOR_ENDPOINT;
        _client = AppServices.Client;
		Connect = new LambdaCommand(onConnect, canConnect);

		reqAccountInfo();
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
		set
		{
			if (Set(ref _isConnected, value))
				NotifyPropertyChanged(nameof(CanConnect));
		}
	}
	public bool CanConnect
	{
		get => !_isConnected;
	}
    #endregion
    #region Commands
	public LambdaCommand Connect { get; }
	private async void onConnect(object? obj)
	{
		var model = IsConnected
			? new ConnectorModel { Host = Host, Port = Port, 
				ClientId = ClientId, IsConnected = false }
			: new ConnectorModel { Host = Host, Port = Port, 
				ClientId = ClientId, IsConnected = true };

        var res = await _client.PostAsJsonAsync(_connectorEndpoint, model);

        if (res.IsSuccessStatusCode)
        {
            model = await res.Content.ReadAsAsync<ConnectorModel>();
            setProperties(model);
        }
    }
	private bool canConnect(object? obj) => !string.IsNullOrEmpty(Host);
    #endregion
}
