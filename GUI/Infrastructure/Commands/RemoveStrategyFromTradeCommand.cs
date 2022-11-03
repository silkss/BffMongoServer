using GUI.Views.Dialogs;
using Strategies.DTO;

namespace GUI.Infrastructure.Commands;

internal class RemoveStrategyFromTradeCommand : Base.Command
{
    public override bool CanExecute(object? parameter) => parameter is MainStrategyDTO;

    public override async void Execute(object? parameter)
    {
        if (parameter is MainStrategyDTO strategy)
        {
            var dlg = new RemoveStrategyDialog(strategy);
            if (dlg.ShowDialog() == true)
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
