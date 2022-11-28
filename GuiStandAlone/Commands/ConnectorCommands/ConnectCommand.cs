using Connectors.Info;
using GuiStandAlone.Views.Dialogs.ConnectorDialogs;

namespace GuiStandAlone.Commands.ConnectorCommands;

internal class ConnectCommand : Base.Command
{
    public override bool CanExecute(object? parameter) => Services.Get.Connector != null;

    public override void Execute(object? parameter)
    {
        var connector = Services.Get.Connector;
        if (connector != null)
        {
            var dlg = new ConnectDialog(connector);
            dlg.ShowDialog();
        }
    }
}
