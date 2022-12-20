using Transactions.Asbstractions;

namespace Strategies.Strategies.Base;

public abstract class TradableStrategy : Strategy, IOrderHolder
{

    public abstract void OnOrderCancelled(int brokerId);

    public abstract void OnOrderFilled(int brokerId);

    public abstract void OnSubmitted(int brokerId);
}
