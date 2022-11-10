using GUI.Views.Dialogs;
using Strategies.DTO;

namespace GUI.Infrastructure.Commands;

internal class OpenModifyDialogCommand : Base.Command
{
    public override bool CanExecute(object? parameter) => parameter is MainStrategyDTO;

    public override async void Execute(object? parameter)
    {
        if (parameter is MainStrategyDTO strategy)
        {
            var dlg = new CreateStrategyDialog(strategy);
            if (dlg.ShowDialog() == true)
            {
                await Services.Get.RequestModifyStrategy(dlg.Strategy);
            }
        }
    }
}
