namespace GUI.Infrastructure.Commands;

internal class RefreshCommand : Base.Command
{
    public override bool CanExecute(object? parameter) => true;

    public override void Execute(object? parameter)
    {
        Services.Get.StrategiesRequests.RefreshAsync();
        Services.Get.TradeRequests.RefreshAsync();
    }
}
