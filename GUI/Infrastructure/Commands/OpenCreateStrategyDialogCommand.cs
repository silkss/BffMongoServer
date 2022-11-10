using GUI.Views.Dialogs;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GUI.Infrastructure.Commands;

internal class OpenCreateStrategyDialogCommand : Base.Command
{
    public override bool CanExecute(object? parameter) => true;
    
    public override async void Execute(object? parameter)
    {
        var dlg = new CreateStrategyDialog(null);
        if (dlg.ShowDialog() == true)
        {
            var strategy = dlg.Strategy;
            var exchange = dlg.cbExchange.SelectedItem.ToString();
            if (string.IsNullOrEmpty(exchange))
            {
                MessageBox.Show("Не указана биржа!",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            var instrument = await Services.Get.InstrumentAsync(dlg.tbLocalSymbol.Text, exchange);
            if (instrument == null)
            {
                MessageBox.Show("Неудалось запросить инструмент. Проверь настройки!", 
                    "Error",
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                return;
            }
            strategy.Instrument = instrument;
            if (await Services.Get.CreateStrategyAsync(strategy))
            {
                Services.Get.RequestAllStrategies();
                return;
            }
            MessageBox.Show("Неудалось создать стратегию. Проверь настройки!",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
        }
    }
}
