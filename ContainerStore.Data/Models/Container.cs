using MongoDB.Bson.Serialization.Attributes;

namespace ContainerStore.Data.Models;

public class Container
{
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string? Id { get; set; }
    public string Account { get; set; }
    public Instrument ParentInstrument { get; set; }
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
    public double ProcentPriceGapForSellClosure { get; set; } = 20;

    public int OrderPriceShift { get; set; } = 2;
}
