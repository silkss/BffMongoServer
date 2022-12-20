using System.Windows;

namespace GreatOptionTrader.Commands.StrategiesCommands;

internal class ShowCreateStrategyDialog : Base.Command
{
    public override bool CanExecute(object? parameter) => parameter is Window;

    public override void Execute(object? parameter)
    {
        if (parameter is Window parent)
        {
            new Views.Strategies.CreateStrategy()
            {
                Owner = parent
            }.Show();
        }
    }
}
