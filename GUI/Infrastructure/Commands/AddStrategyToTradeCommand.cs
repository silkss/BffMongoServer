using Strategies.DTO;

namespace GUI.Infrastructure.Commands;

internal class AddStrategyToTradeCommand : Base.Command
{
    public override bool CanExecute(object? parameter) => parameter is MainStrategyDTO;

    public override async void Execute(object? parameter)
    {
        if (parameter is MainStrategyDTO strategy)
        {
            var res = await Services.Get.RequestAddToTrader(strategy.Id);
            if (res)
            {
                Services.Get.RequestStrategiesInTrade();
            }
        }
    }
}
