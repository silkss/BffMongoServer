using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using ContainerStore.Common.Enums;
using ContainerStore.Data.Infrastructure.Wpf;
using ContainerStore.Data.Models.Instruments;
using MongoDB.Bson.Serialization.Attributes;

namespace ContainerStore.Data.Models;

public class Container : NotifyProperty
{
    private readonly object _lock = new();

    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string? Id { get; set; }
    public string Account { get; set; }

    private Instrument _parentInstrument;
    public Instrument ParentInstrument
    {
        get => _parentInstrument;
        set => Set(ref _parentInstrument, value);
    }

    public decimal TotalPnl { get; set; }
    public decimal StraddleTargetPnl { get; set; } = 300.00m;

    /// <summary>
    /// StraddleLiveDays -- сколько максимум дней может быть открыт опцион.
    /// Чтобы стрэддл закрылся ему необходимо выполнение одно из двух условий:
    /// 1. Должен набраться Pnl <see cref="TargetPnlForStraddles"/>
    /// 2. Должен прожить не меньше <see cref="StraddleLiveDays"/> дней
    /// </summary>
    public int StraddleLiveDays { get; set; } = 2;

    /// <summary>
    /// StraddleExpirationDays минимальное "растояние" до даты экспирации.
    /// Указывается для того, чтобы выбирать максималльно удачный, по дате экспирации, 
    /// инструмент (опционы) для стреддла.
    /// </summary>
    public int StraddleExpirationDays { get; set; } = 10;

    /// <summary>
    /// ClosureStrikeStep - шаг страйка, необходим для выбора страйка замыкающей стратегии.
    /// </summary>
    public int ClosureStrikeStep { get; set; } = 2;

    /// <summary>
    /// ProcentPriceGapForSellClosure - берет процент от цены покупки основного опциона, 
    /// и на основе его высчитывает цену продажи замыкающего опциона.
    /// </summary>
    public int ClosurePriceGapProcent { get; set; } = 110;

    [BsonIgnore]
    [JsonIgnore]
    public Straddle? OpenStraddle => Straddles.FirstOrDefault(s => s.Logic == TradeLogic.Open);
    [JsonIgnore]
    [BsonIgnore]
    public DateTime? ApproximateCloseDate => OpenStraddle?.CreatedTime.AddDays(StraddleLiveDays);
    
    [BsonIgnore]
    public decimal CurrencyPnl => Straddles.Sum(s => s.CurrencyPnl);
    [BsonIgnore]
    public decimal CurrencyOpenPnl => Straddles
        .FirstOrDefault(s => s.Logic == TradeLogic.Open) is Straddle straddle
            ? straddle.CurrencyPnl
            : 0m;
    public int OrderPriceShift { get; set; } = 2;
    public List<Straddle> Straddles { get; private set; } = new();
    public decimal Pnl => Straddles.Sum(s => s.GetPnl());

    public void AddStraddle(Straddle straddle)
    {
        lock (_lock)
        {
            if (Straddles == null) Straddles = new();
            Straddles.Add(straddle);
        }
    }
    public void Close()
    {
        foreach (var straddle in Straddles)
        {
            straddle.Close();
        }
    }

    /// <summary>
    ///  Дата, больше которой должен быть экспирация опциона при добавлении стрэддла.
    /// </summary>
    /// <returns></returns>
    public DateTime GetApproximateExpirationDate() => DateTime.Now.AddDays(StraddleExpirationDays);
    public void Stop()
    {
        foreach (var straddle in Straddles)
        {
            straddle.Stop();
        }
    }
}
