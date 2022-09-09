using System.Windows;

namespace ContainerStore.Gui.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainView : Window
{
    public MainView()
    {
        InitializeComponent();
    }

    private void showConnectorView(object sender, RoutedEventArgs e)
    {
        var connectorView = new ConnectorView
        {
            Owner = this,
        };
        connectorView.Show();
    }

    private void showContainersView(object sender, RoutedEventArgs e)
    {
        var view = new ContainersView
        {
            Owner = this,
        };
        view.Show();
    }
}
