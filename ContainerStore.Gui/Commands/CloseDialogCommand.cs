using ContainerStore.Gui.Commands.Base;
using System.Windows;

namespace ContainerStore.Gui.Commands;

internal class CloseDialogCommand : Command
{
    public bool? DialogResult { get; set; }

    public override bool CanExecute(object? parameter) => parameter is Window;

    public override void Execute(object? parameter)
    {
        if (parameter == null) return;

        var window = (Window)parameter;
        window.DialogResult = DialogResult;
        window.Close();
    }
}
