using System.Windows.Controls;

namespace GUI.Views.Controls;

/// <summary>
/// Логика взаимодействия для AllStrategies.xaml
/// </summary>
public partial class AllStrategies : UserControl
{
    public AllStrategies()
    {
        InitializeComponent();
    }

    private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        Services.Get.StrategiesRequests.RefreshAsync();
    }
}
