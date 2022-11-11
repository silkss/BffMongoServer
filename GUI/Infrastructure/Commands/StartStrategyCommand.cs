using Strategies.DTO;

namespace GUI.Infrastructure.Commands;

internal class StartStrategyCommand : Base.Command
{
    public override bool CanExecute(object? parameter) => parameter is MainStrategyDTO;

    public override async void Execute(object? parameter)
    {
        if (parameter is MainStrategyDTO strategy)
        {
            var res = await Services.Get.TradeRequests.StartStrategy(strategy.Id);
            if (res)
            {
                Services.Get.TradeRequests.RefreshAsync();
            }
        }
    }
}
