namespace GUI.Infrastructure.Commands;

internal class RefreshCommand : Base.Command
{
    public override bool CanExecute(object? parameter) => true;

    public override async void Execute(object? parameter)
    {
        Services.Get.StrategiesRequests.RefreshAsync();
        Services.Get.StrategiesRequests.RefreshAsync();
    }
}
