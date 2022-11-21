using GUI.Views.Dialogs;
using Strategies.DTO;

namespace GUI.Infrastructure.Commands;

internal class ShowPlotViewCommand : Base.Command
{
    public override bool CanExecute(object? parameter) => parameter is MainStrategyDTO;

    public override void Execute(object? parameter)
    {
        if (parameter is MainStrategyDTO strategy)
        {
            new PlotViewDialog(strategy).ShowDialog();
        }
    }
}
