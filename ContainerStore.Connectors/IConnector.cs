using ContainerStore.Common.Enums;
using ContainerStore.Connectors.Info;
using Instruments;
using System;
using System.Collections.Generic;
using Transactions;

namespace ContainerStore.Connectors;

public interface IConnector
{
    #region Connector Props
    ConnectorInfo GetConnectionInfo();
    IEnumerable<string> GetAccounts();
    #endregion
    #region Connect/Disconnect
    void Connect();
    void Connect(string host, int port, int clientId);
    void Disconnect();
    void AddConnectionChangedCallback(Action<bool> callback);
    #endregion
    #region Instruments Requests
    Instrument? RequestInstrument(string fullname, string exchange);
    IConnector RequestDependentInstrument(
        InstrumentType type, 
        OptionType optionType, 
        Instrument parent, 
        double strike,
        DateTime expDate, 
        out Instrument? instrument);
    IConnector RequestCall(Instrument parent, double strike, DateTime expirationDate, out Instrument? instrument);
    IConnector RequestPut(Instrument parent, double strike, DateTime expirationDate, out Instrument? instrument);
    IConnector RequestOptionChain(Instrument instrument);
    IConnector RequestMarketData(Instrument? instrument);
    OptionTradingClass? GetOptionTradingClass(int parentId, DateTime approximateDate);
    void ReqMarketRule(int id);
    #endregion

    #region Transactions/ Orders

    bool IsOrderOpen(Transaction order);
    void SendLimitOrder(Instrument instrument, Transaction order, int priceShift = 0, bool needToRounds = true);
    IConnector CancelOrder(Transaction? transaction);
    #endregion
}
