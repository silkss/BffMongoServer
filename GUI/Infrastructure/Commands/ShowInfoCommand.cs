using GUI.Views.Dialogs;
using Strategies.DTO;

namespace GUI.Infrastructure.Commands;

internal class ShowInfoCommand : Base.Command
{
    public override bool CanExecute(object? parameter) => parameter is MainStrategyDTO;

    public override async void Execute(object? parameter)
    {
        if (parameter is MainStrategyDTO strategy)
        {
            var info = await Services.Get.TradeRequests.GetStrategy(strategy.Id);
            if (info != null)
            {
                new StrategyInfo(info).ShowDialog();
            }    
        }
    }
}
