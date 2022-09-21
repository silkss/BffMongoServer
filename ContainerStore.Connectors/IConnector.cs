﻿using ContainerStore.Common.Enums;
using ContainerStore.Data.Models.Accounts;
using ContainerStore.Data.Models.Instruments;
using ContainerStore.Data.Models.Transactions;
using ContainerStore.Data.ServiceModel;
using System;
using System.Collections.Generic;

namespace ContainerStore.Connectors;

public interface IConnector
{
    #region Connector Props
    ConnectorModel GetConnectionInfo();
    IEnumerable<Account> GetAccounts();
    #endregion
    #region Connect/Disconnect
    void Connect();
    void Connect(string host, int port, int clientId);
    void Disconnect();
    void AddConnectionChangedCallback(Action<bool> callback);
    #endregion
    #region Instruments Requests
    Instrument? RequestInstrument(string fullname, string exchange);
    Instrument? RequestDependentInstrument(InstrumentType type, OptionType optionType, Instrument parent, double strike, DateTime expDate);
    Instrument? RequestCall(Instrument parent, double strike, DateTime expirationDate);
    Instrument? RequestPut(Instrument parent, double strike, DateTime expirationDate);
    IConnector RequestOptionChain(Instrument instrument);
    IConnector RequestMarketData(Instrument? instrument);
    OptionTradingClass? GetOptionTradingClass(int parentId, DateTime approximateDate);
    void ReqMarketRule(int id);
    #endregion

    #region Transactions/ Orders
    void SendOrder(Instrument instrument, Transaction order, decimal price, int priceShift);
    void CancelOrder(Transaction transaction);
    #endregion
}
