using GUI.Views.Dialogs;
using Strategies.DTO;
using System.Windows;

namespace GUI.Infrastructure.Commands;

internal class DeleteStrategyCommand : Base.Command
{
    public override bool CanExecute(object? parameter) => parameter is MainStrategyDTO;

    public override async void Execute(object? parameter)
    {
        if (parameter is MainStrategyDTO strategy)
        {
            if (MessageBox.Show($"Уверен что хочешь УДАЛИТЬ стратегию?",
                $"{strategy.Instrument?.FullName} | {strategy.MainSettings?.Account} | {strategy.PnlCurrency}",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if(await Services.Get.StrategiesRequests.RemoveAsync(strategy.Id))
                {
                    Services.Get.StrategiesRequests.RefreshAsync();
                }
            }
        }
    }
}
