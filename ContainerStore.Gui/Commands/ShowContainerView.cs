using ContainerStore.Gui.Commands.Base;
using ContainerStore.Gui.Views;
using Strategies;

namespace ContainerStore.Gui.Commands;

internal class ShowContainerView : Command
{
    public bool IsContainerInTrade { get; set; }
    public override bool CanExecute(object? parameter) => parameter is MainStrategy;

    public override void Execute(object? parameter)
    {
        if (parameter is MainStrategy strategy)
        {
            new ContainerView
            {
                IsContainerInTrade = IsContainerInTrade,
                Container = strategy,
                Owner = App.Current.MainWindow,
            }.Show();
        }
    }
}
