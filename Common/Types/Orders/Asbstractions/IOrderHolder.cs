namespace Common.Types.Orders.Asbstractions;

public interface IOrderHolder
{
    void OnOrderFilled(int brokerId);
    void OnOrderCancelled(int brokerId);
    void OnSubmitted(int brokerId);
}
