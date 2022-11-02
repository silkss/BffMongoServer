using System;

namespace GUI.Infrastructure.Commands.Base;

internal class LambdaCommand : Command
{
    private readonly Action<object?> _action;
    private readonly Predicate<object?>? _predicate;

    public LambdaCommand(Action<object?> action, Predicate<object?>? predicate = null)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action));
        _predicate = predicate;
    }

    public override bool CanExecute(object? parameter) => _predicate?.Invoke(parameter) ?? true;

    public override void Execute(object? parameter) => _action(parameter);
}
