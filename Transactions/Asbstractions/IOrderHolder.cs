namespace Transactions.Asbstractions;

public interface IOrderHolder
{
    void OnOrderFilled(int brokerId);
    void OnOrderCancelled(int brokerId);
    void OnSubmitted(int brokerId);
}
