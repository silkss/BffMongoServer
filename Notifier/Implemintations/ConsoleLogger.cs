using System;

namespace Notifier.Implemintations;

public class ConsoleLogger : Base.Logger
{
    protected override void Log(LogLevel level, string msg) =>
        Console.WriteLine($"{level}\t{msg}");
}
