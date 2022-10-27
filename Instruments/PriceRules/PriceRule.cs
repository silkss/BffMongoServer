using System.Collections.Generic;

namespace Instruments.PriceRules;

public class PriceRule
{
    public PriceRule(int id)
    {
        Id = id;
        Borders = new();
    }
    public int Id { get; set; }
    public List<PriceBorder> Borders { get; private set; }
    public void ChangeBorders(List<PriceBorder> borders)
    {
        Borders = borders;
    }
}

public class PriceBorder
{
    //минимальное значение цены, при которой действует инкремент.
    public decimal LowEdge { get; set; }
    public decimal Incriment { get; set; }
}