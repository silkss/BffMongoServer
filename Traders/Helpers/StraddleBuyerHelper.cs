using Connectors;
using Strategies.Strategies;
using Strategies.Strategies.TradeUnions;
using System;
using System.Linq;
using System.Text;

namespace Traders.Helpers;

public static class StraddleBuyerHelper
{
    /// <summary>
    /// Время, после которого в пятницу ничего не происходит.
    /// </summary>
    private static TimeSpan _fridayDeadLine = new TimeSpan(hours: 12, minutes: 00, seconds: 00);

    public static string OpenStraddle(IConnector connector, MainStrategy mainStrategy, double price)
    {

        if (DateTime.Now.DayOfWeek == DayOfWeek.Friday && DateTime.Now.TimeOfDay >= _fridayDeadLine)
        {
            return $"Cant open straddle. Because: {DateTime.Now.DayOfWeek} {DateTime.Now.TimeOfDay}";
        }
        var optionclass = connector
            .GetOptionTradingClass(mainStrategy.Instrument.Id, mainStrategy.GetApproximateExpirationDate());
        if (optionclass == null)
        {
            return "Нет подходящего опционного класса.";
        }
        var baseStrike = optionclass.Strikes.MinBy(s => Math.Abs(s - price));
        var baseStrikeIdx = optionclass.Strikes.FindIndex(s => s == baseStrike);
        var closureCallStike = optionclass
            .Strikes[baseStrikeIdx + (mainStrategy.ClosureSettings?.ClosureStrikeStep ?? 0)];

        var closurePutStrike = optionclass
            .Strikes[baseStrikeIdx - (mainStrategy.ClosureSettings?.ClosureStrikeStep ?? 0)];

        connector
            .RequestCall(mainStrategy.Instrument, baseStrike, optionclass.ExpirationDate, out var baseCall)
            .RequestCall(mainStrategy.Instrument, closureCallStike, optionclass.ExpirationDate, out var closureCall)
            .RequestPut(mainStrategy.Instrument, baseStrike, optionclass.ExpirationDate, out var basePut)
            .RequestPut(mainStrategy.Instrument, closurePutStrike, optionclass.ExpirationDate, out var closurePut);

        if (baseCall is null)
        {
            return $"не удалось запросить Call для ноги стредла:\n" +
                $"parentId: {mainStrategy.Instrument.Id}. Strike:{baseStrike}. ExpDate: {optionclass.ExpirationDate}.";

        }
        if (closureCall is null)
        {
            return $"не удалось запросить Call для замыкания:\n" +
                $"parentId: {mainStrategy.Instrument.Id}. Strike:{baseStrike}. ExpDate: {optionclass.ExpirationDate}.";
        }
        if (basePut is null)
        {
            return $"не удалось запросить Put для ноги стредла:\n" +
                $"parentId: {mainStrategy.Instrument.Id}. Strike:{baseStrike}. ExpDate: {optionclass.ExpirationDate}.";
        }
        if (closurePut is null)
        {
            return $"не удалось запросить Put для замыкания:\n" +
                $"parentId: {mainStrategy.Instrument.Id}. Strike:{baseStrike}. ExpDate: {optionclass.ExpirationDate}.";
        }

        connector
            .RequestMarketData(baseCall)
            .RequestMarketData(basePut)
            .RequestMarketData(closureCall)
            .RequestMarketData(closurePut);

        mainStrategy.AddStraddle(new Straddle(baseCall, closureCall, basePut, closurePut), connector);
        return "Straddle добавлен.";
    }

    public static string CloseStraddle(IConnector connector, Straddle? straddle, string message)
    {
        if (straddle is null) throw new ArgumentNullException("While closing straddle it cant be null!");
        straddle.Close(connector);
        return $"Closing straddle. Reasong: {message}";
    }
    public static string CloseAndOpenStraddle(
        IConnector connector,
        MainStrategy strategy,
        double price,
        string message) => new StringBuilder()
			.AppendLine(CloseStraddle(connector, strategy.GetOpenStraddle(), message))
			.AppendLine(OpenStraddle(connector, strategy, price))
			.ToString();
}