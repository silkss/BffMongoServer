using ContainerStore.Data.ServiceModel;
using ContainerStore.Gui.Services;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;


namespace ContainerStore.Gui.Views;

/// <summary>
/// Логика взаимодействия для Connector.xaml
/// </summary>
public partial class ConnectorView : Window
{
    private const string PATH = "api/connector";
    private void setProperties(ConnectorModel model)
    {
        tbHost.Text = model.Host;
        tbPort.Text = model.Port.ToString();
        tbClientId.Text = model.ClientId.ToString();
        lConnected.Content = model.IsConnected.ToString();
    }
    public ConnectorView()
    {
        InitializeComponent();
    }

    private async void onWindowLoaded(object sender, RoutedEventArgs e)
    {
        var client = AppContext.Client;
        HttpResponseMessage response = await client.GetAsync(PATH);
        if (response.IsSuccessStatusCode)
        {
            var model = await response.Content.ReadAsAsync<ConnectorModel>();
            setProperties(model);
        }
    }

    private async void Connect(object sender, RoutedEventArgs e)
    {
        var model = new ConnectorModel
        {
            Host = tbHost.Text,
            Port = int.Parse(tbPort.Text),
            ClientId = int.Parse(tbClientId.Text),
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
    private async void Disconnect(object sender, RoutedEventArgs e)
    {
        var client = AppContext.Client;
        var model = new ConnectorModel
        {
            Host = tbHost.Text,
            Port = int.Parse(tbPort.Text),
            ClientId = int.Parse(tbClientId.Text),
            IsConnected = false,
        };
        var res = await client.PostAsJsonAsync(PATH, model);

        if (res.IsSuccessStatusCode)
        {
            model = await res.Content.ReadAsAsync<ConnectorModel>();
            setProperties(model);
        }
    }
}
