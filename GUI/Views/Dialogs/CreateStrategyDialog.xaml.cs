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
        }
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

    private async void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (cbExchange.SelectedItem is string exchange)
        {
            var instrument = await Services.Get.InstrumentAsync(tbLocalSymbol.Text, exchange);
            if (instrument == null)
            {
                tbError.Text = "Неудалось запросить инструмент. Проверь настройки!";
                return;
            }
            Strategy.Instrument = instrument;
            if (await Services.Get.CreateStrategyAsync(Strategy))
            {
                DialogResult = true;
            }
            tbError.Text = "Чтото не так с создание стратегии!";
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
