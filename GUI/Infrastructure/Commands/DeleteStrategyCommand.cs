using GUI.Views.Dialogs;
using Strategies.DTO;

namespace GUI.Infrastructure.Commands;

internal class DeleteStrategyCommand : Base.Command
{
    public override bool CanExecute(object? parameter) => parameter is MainStrategyDTO;

    public override void Execute(object? parameter)
    {
        if (parameter is MainStrategyDTO strategy)
        {
            var dlg = new RemoveStrategyDialog(strategy);
            if (dlg.ShowDialog() == true)
            {
                Services.Get.RequestAllStrategies();
            }
        }
    }
}
