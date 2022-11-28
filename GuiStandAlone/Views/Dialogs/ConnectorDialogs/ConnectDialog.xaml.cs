using Connectors;
using Connectors.Info;
using System.Windows;

namespace GuiStandAlone.Views.Dialogs.ConnectorDialogs;

/// <summary>
/// Логика взаимодействия для ConnectDialog.xaml
/// </summary>
public partial class ConnectDialog : Window
{
    public ConnectDialog(IConnector connector)
    {
        InitializeComponent();
        _connector = connector;
        ConnInfo = _connector.GetConnectionInfo();
    }

    public static readonly DependencyProperty ConnInfoProperty = DependencyProperty.Register(
        nameof(ConnInfo),
        typeof(ConnectorInfo),
        typeof(ConnectDialog),
        new PropertyMetadata(null));
    private readonly IConnector _connector;

    public ConnectorInfo ConnInfo
    {
        get => (ConnectorInfo)GetValue(ConnInfoProperty);
        set => SetValue(ConnInfoProperty, value);
    }

    private void Connect_Click(object sender, RoutedEventArgs e) =>
        _connector.Connect(ConnInfo.Host, ConnInfo.Port, ConnInfo.ClientId);
    private void Disconnect_Click(object sender, RoutedEventArgs e) =>
        _connector.Disconnect();
    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
