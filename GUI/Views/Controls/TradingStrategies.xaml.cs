using System.Windows.Controls;

namespace GUI.Views.Controls;

/// <summary>
/// Логика взаимодействия для TradingStrategies.xaml
/// </summary>
public partial class TradingStrategies : UserControl
{
    public TradingStrategies()
    {
        InitializeComponent();
    }

    private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        Services.Get.RequestStrategiesInTrade();
    }
}
