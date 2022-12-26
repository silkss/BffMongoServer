using Common.Types.Base;
using Common.Types.Instruments;
using Connectors.Info;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Connectors;

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
    Task<Instrument?> RequestInstrumentAsync(string fullname, string exchange);
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
    IEnumerable<OptionTradingClass> GetOptionTradingClasses(Instrument parent, string tradingClass);
    void ReqMarketRule(int id);
    #endregion

    #region Transactions/ Orders
    bool IsOrderOpen(Common.Types.Orders.Order order);
    void SendLimitOrder(Instrument instrument, Common.Types.Orders.Order order, int priceShift = 0, bool needToRounds = true);
    IConnector CancelOrder(Common.Types.Orders.Order? order);
    #endregion
}
