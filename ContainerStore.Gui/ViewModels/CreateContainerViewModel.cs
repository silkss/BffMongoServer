using ContainerStore.Data.Models;
using ContainerStore.Data.Models.Instruments;
using ContainerStore.Data.ServiceModel;
using ContainerStore.Gui.Commands;
using ContainerStore.Gui.Services;
using ContainerStore.Gui.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;

namespace ContainerStore.Gui.ViewModels;

internal class CreateContainerViewModel : ViewModel
{
    private readonly string _instrumentEndpoint;// INSTRUMENT_PATH = "api/instrument";
    private readonly string _connectorEndpoint;// CONNECTOR_PATH = "api/connector";
    private readonly string _containerEndpoint;// CONTAINER_PATH = "api/containers";

    private readonly HttpClient _client;
    private async void reqAccounts()
    {
        var  response = await  _client.GetAsync(_connectorEndpoint);
        if (response.IsSuccessStatusCode)
        {
            if (await response.Content.ReadAsAsync<ConnectorModel>() is ConnectorModel connector)
            {
                if (connector.Accounts == null) return;
                foreach (var account in connector.Accounts)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Accounts.Add(account.Name);
                    });
                }
            }
        }
    }
    public CreateContainerViewModel()
    {
        _instrumentEndpoint = AppServices.INSTRUMENT_ENDPOINT;
        _connectorEndpoint = AppServices.CONNECTOR_ENDPOINT;
        _containerEndpoint = AppServices.CONTAINER_ENDPOINT;

        _client = AppServices.Client;

        ReqInstrument = new LambdaCommand(onReqInstrument, canRequest);
        Create = new LambdaCommand(onCreatedAsync, canCreate);

        reqAccounts();
    }
    #region Commands
    public LambdaCommand ReqInstrument { get; }
    private void onReqInstrument(object? obj)
    {
        HttpResponseMessage response = _client
            .GetAsync(_instrumentEndpoint + $"?localname={InstrumentName}&exchange={Exchange}").Result;
        if (response.IsSuccessStatusCode)
        {
            Container.ParentInstrument = response.Content.ReadAsAsync<Instrument>().Result;
        }
    }
    private bool canRequest(object? obj) => !string.IsNullOrEmpty(InstrumentName)
        && !string.IsNullOrEmpty(Exchange);
    public LambdaCommand Create { get; }
    private async void onCreatedAsync(object? obj)
    {
        var res = await _client.PostAsJsonAsync(_containerEndpoint, Container);

        if (res.IsSuccessStatusCode)
        {
            Debug.WriteLine($"Added::{res.StatusCode}");
        }
    }
    private bool canCreate(object? obj) => Container.ParentInstrument != null && !string.IsNullOrEmpty(Container.Account);
    #endregion
    #region Props
    public Container Container { get; } = new Container();
    public ObservableCollection<string> Accounts { get; } = new();
    private string _instrumentName;
    public string InstrumentName
    {
        get => _instrumentName;
        set => Set(ref _instrumentName, value);
    }

    private string _exchange = "GLOBEX";
    public string Exchange
    {
        get => _exchange;
        set => Set(ref _exchange, value);
    }
    #endregion
}
