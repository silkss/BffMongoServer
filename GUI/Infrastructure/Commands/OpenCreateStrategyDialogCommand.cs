using GUI.Views.Dialogs;

namespace GUI.Infrastructure.Commands;

internal class OpenCreateStrategyDialogCommand : Base.Command
{
    public override bool CanExecute(object? parameter) => true;
    
    public override void Execute(object? parameter)
    {
        var dlg = new CreateStrategyDialog(null);
        if (dlg.ShowDialog() == true)
        {
            Services.Get.RequestAllStrategies();
        }
    }
}
