using Connectors.Info;
using System.Windows;

namespace GUI.Views.Dialogs;

/// <summary>
/// Логика взаимодействия для ConnectorDialog.xaml
/// </summary>
public partial class ConnectorDialog : Window
{
    public static readonly DependencyProperty ConnectorInfoProperty = DependencyProperty.Register(
    nameof(ConnectorInfo),
    typeof(ConnectorInfo),
    typeof(ConnectorDialog),
    new PropertyMetadata(null));

    public ConnectorInfo? ConnectorInfo
    {
        get => (ConnectorInfo?)GetValue(ConnectorInfoProperty);
        set => SetValue(ConnectorInfoProperty, value);
    }
    public ConnectorDialog()
    {
        InitializeComponent();

        ConnectorInfo = Services.Get.ConnectorInfo;
    }

    private async void Connect(object sender, RoutedEventArgs e)
    {
        if (await Services.Get.ConnectorRequests.ConnectAsync())
        {
            tbError.Text = "Connected!";
        }
        else
        {
            tbError.Text = "Some error whilee connecting!";
        }
    }

    private async void Disconnet(object sender, RoutedEventArgs e)
    {
        if (await Services.Get.ConnectorRequests.DisconnectAsync())
        {
            tbError.Text = "Disconnected!";
        }
        else
        {
            tbError.Text = "Some error whilee disconnecting!";
        }
    }

    private async void ReqInfo_Click(object sender, RoutedEventArgs e)
    {
        ConnectorInfo = await Services.Get.ConnectorRequests.GetConnectorInfo();
    }
}
