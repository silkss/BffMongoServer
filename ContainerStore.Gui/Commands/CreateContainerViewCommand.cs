using ContainerStore.Gui.Commands.Base;
using ContainerStore.Gui.Views;
using System.Windows;

namespace ContainerStore.Gui.Commands;

internal class CreateContainerViewCommand : Command
{
    public override bool CanExecute(object? parameter) => parameter != null;

    public override void Execute(object? parameter)
    {
        if (parameter == null) return; 
        var view = new CreateContainerView()
        {
            Owner = (Window)parameter,
        };
        view.Show();
    }
}
