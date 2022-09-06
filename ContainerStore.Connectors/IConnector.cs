using ContainerStore.Connectors.Models;

namespace ContainerStore.Connectors;

public interface IConnector
{
    string Host { get; }
    int Port { get; }
    int ClientId { get; }
    bool IsConnected { get; }
    void Connect();
    void Connect(string host, int port, int clientId);
    void Disconnect();
    IEnumerable<Account> GetAccounts();
}
