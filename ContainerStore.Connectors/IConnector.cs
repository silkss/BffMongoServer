using ContainerStore.Data.Models;
using ContainerStore.Data.Models.Accounts;
using System.Collections.Generic;

namespace ContainerStore.Connectors;

public interface IConnector
{
    #region Connector Props
    string Host { get; }
    int Port { get; }
    int ClientId { get; }
    bool IsConnected { get; }
    IEnumerable<Account> GetAccounts();
    #endregion
    #region Connect/Disconnect
    void Connect();
    void Connect(string host, int port, int clientId);
    void Disconnect();
    #endregion
    #region Instruments Requests
    Instrument? RequestInstrument(string fullname, string exchange);
    void RequestOptionChain(Instrument instrument);
    void RequestMarketData(Instrument instrument);
    
    #endregion
}
