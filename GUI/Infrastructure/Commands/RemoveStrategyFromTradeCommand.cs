using GUI.Views.Dialogs;
using Strategies.DTO;
using System.Windows;

namespace GUI.Infrastructure.Commands;

internal class RemoveStrategyFromTradeCommand : Base.Command
{
    public override bool CanExecute(object? parameter) => parameter is MainStrategyDTO;

    public override async void Execute(object? parameter)
    {
        if (parameter is MainStrategyDTO strategy)
        {
            if (MessageBox.Show($"Уверен что хочешь оставноть стратегию?",
                $"{strategy.Instrument?.FullName} | {strategy.MainSettings?.Account} | {strategy.PnlCurrency}", 
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (await Services.Get.RequestRemoveFromTraade(strategy.Id))
                {
                    Services.Get.RequestStrategiesInTrade();
                    Services.Get.RequestAllStrategies();
                }
            }
        }
    }
}
