using Strategies.DTO;
using System.Windows;

namespace GUI.Infrastructure.Commands;

internal class CloseCommand : Base.Command
{
    public override bool CanExecute(object? parameter) => parameter is MainStrategyDTO;

    public override void Execute(object? parameter)
    {
        if (parameter is MainStrategyDTO strategy)
        {
            if (MessageBox.Show("Уверен что хочешь вручную закрыть стратегию?",
                $"{strategy.Instrument?.FullName} | {strategy.MainSettings?.Account} | {strategy.PnlCurrency}",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                
            }
        }
    }
}
