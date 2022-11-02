using GUI.Views.Dialogs;
using System.Windows;

namespace GUI.Infrastructure.Commands;

internal class OpenConnectorSettingsCommand : Base.Command
{
    public override bool CanExecute(object? parameter) => parameter is Window;

    public override void Execute(object? parameter) 
    {
        if (parameter is Window owner)
        {
            var dlg = new ConnectorDialog()
            {
                Owner = owner,
            };
            dlg.ShowDialog();
        }
    }
}
