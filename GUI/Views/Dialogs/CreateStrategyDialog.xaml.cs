using Strategies.DTO;
using Strategies.Settings;
using Strategies.Settings.Straddle;
using System.Windows;

namespace GUI.Views.Dialogs;

/// <summary>
/// Логика взаимодействия для CreateStrategyDialog.xaml
/// </summary>
public partial class CreateStrategyDialog : Window
{
    public CreateStrategyDialog(MainStrategyDTO? mainStrategy)
    {
        InitializeComponent();
        
        cbExchange.ItemsSource = Services.Get.Exchanges;
        cbAccounts.ItemsSource = Services.Get.ConnectorInfo?.Accounts;

        if (mainStrategy == null)
        {
            IsModifying = true;
            Strategy = new MainStrategyDTO
            {
                MainSettings = new MainSettings(),
                ClosureSettings = new ClosureSettings(),
                StraddleSettings = new StraddleSettings()
            };
            
        }
        else
        {
            Strategy = mainStrategy;

            cbExchange.IsEnabled = false;
            cbExchange.SelectedItem = Strategy.Instrument?.Exchange;

            cbAccounts.IsEnabled = false;
            
            tbLocalSymbol.IsEnabled = false;
            tbLocalSymbol.Text = Strategy.Instrument?.FullName;

            OkButton.Content = "Update";
        }
    }

    public static readonly DependencyProperty IsModifyingProperty = DependencyProperty.Register(
        nameof(IsModifying),
        typeof(bool),
        typeof(CreateStrategyDialog),
        new PropertyMetadata(null));

    public bool IsModifying
    {
        get => (bool)GetValue(IsModifyingProperty);
        set => SetValue(IsModifyingProperty, value);
    }

    public static readonly DependencyProperty StrategyProperty = DependencyProperty.Register(
        nameof(Strategy),
        typeof(MainStrategyDTO),
        typeof(CreateStrategyDialog),
        new PropertyMetadata(null));

    public MainStrategyDTO Strategy
    {
        get => (MainStrategyDTO)GetValue(StrategyProperty);
        set => SetValue(StrategyProperty, value);
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
