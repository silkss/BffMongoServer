using System.Windows;

namespace GreatOptionTrader.Commands.ContainerCommands;

internal class Create : Base.Command
{
    public override bool CanExecute(object? parameter) => parameter is Window;

    public override void Execute(object? parameter)
    {
        if (parameter is Window parent)
        {
            new Views.Containers.CreateContainer()
            {
                Owner = parent
            }.Show();
        }
    }
}
