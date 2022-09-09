﻿using Microsoft.Extensions.Logging;
using ContainerStore.Connectors.Ib.Caches;
using ContainerStore.Connectors.Converters.Ib;
using System.Threading;
using System.Collections.Generic;
using IBApi;
using ContainerStore.Data.Models;
using ContainerStore.Data.Models.Accounts;
using ContainerStore.Data.Models.Instruments;
using System;
using ContainerStore.Common.Enums;
using ContainerStore.Data.Models.Transactions;

namespace ContainerStore.Connectors.Ib;

public class IbConnector : IConnector
{
    private readonly RequestInstrumentCache _requestInstrument = new();
    private readonly OpenOrdersCache _openOrdersCache = new();
	private readonly IbCallbacks _callbacks;
    private readonly EClientSocket _client;
    private readonly EReaderSignal _signalMonitor = new EReaderMonitorSignal();
    private readonly ILogger<IbConnector> _logger;
    private readonly Dictionary<int, OptionChain> _optionChains = new();
    private Instrument? reqContract(Contract contract)
    {
        var reqid = _callbacks.NextOrderId++;
        _client.reqContractDetails(reqid, contract);

        while (!_requestInstrument.ContainsKey(reqid))
        {
            _requestInstrument.WaitForResponce();
        }
        return _requestInstrument.GetByKey(reqid);
    }

    public IbConnector(ILogger<IbConnector> logger)
	{
        _logger = logger;

		_callbacks = new IbCallbacks(_logger, _requestInstrument, _optionChains, _openOrdersCache);
        _client = new EClientSocket(_callbacks, _signalMonitor);
    }
    #region Connector props
    public string Host { get; private set; } = "127.0.0.1";
    public int Port { get; private set; } = 7497;
    public int ClientId { get; private set; } = 12;
    public bool IsConnected { get => _client.IsConnected(); }
    public IEnumerable<Account> GetAccounts() => _callbacks.Accounts;
    #endregion
    #region Connect / Disconnect
    public void Connect()
    {
        Connect(Host, Port, ClientId);
    }
    public void Connect(string host, int port, int clientId)
    {
        Host = host;
        Port = port;
        ClientId = clientId;
        _client.eConnect(Host, Port, ClientId);

        var reader = new EReader(_client, _signalMonitor);
        reader.Start();

        new Thread(() =>
        {
            while (_client.IsConnected())
            {
                _signalMonitor.waitForSignal();
                reader.processMsgs();
            }
        })
        { IsBackground = true }
        .Start();
        if (!_client.IsConnected()) return;

        _client.reqMarketDataType(3);
    }
    public void Disconnect()
    {
        _client.eDisconnect();
    }
    #endregion
    #region Instruments and etc
    public Instrument? RequestInstrument(string fullname, string exchange)
    {
        var contract = new Contract
        {
            LocalSymbol = fullname.Trim().ToUpper(),
            Exchange = exchange.Trim().ToUpper(),
            SecType = "FUT",
            Currency = "USD",
        };
        return reqContract(contract);
    }
    public Instrument? RequestDependentInstrument(InstrumentType type, OptionType optionType, Instrument parent, double strike, DateTime expDate)
    {
        var contract = new Contract
        {
            Strike = strike,
            LastTradeDateOrContractMonth = expDate.ToString("yyyyMMdd"),
            SecType = "FOP",
            Symbol = parent.Symbol,
            Exchange = parent.Exchange,
            Currency = parent.Currency,
            Right = optionType == OptionType.Call ? "C" : "P"

        };
        /* NG иммеет 2 одинаковый опционых серии.
         * 1ая поставочная
         * 2ая расчетная.
         * нам нужна 1ая.
         * Онаже LNE
         */
        if (contract.Symbol == "NG")
        {
            contract.TradingClass = "LNE";
        }
        return reqContract(contract);
    }
    public Instrument? RequestCall(Instrument parent, double strike, DateTime expirationDate) =>
        RequestDependentInstrument(InstrumentType.Option, OptionType.Call, parent, strike, expirationDate);
    public Instrument? RequestPut(Instrument parent, double strike, DateTime expirationDate) =>
        RequestDependentInstrument(InstrumentType.Option, OptionType.Put, parent, strike, expirationDate);
    public IConnector RequestOptionChain(Instrument instrument)
    {
        if (instrument.Type != InstrumentType.Future) return this;
        var oc = _optionChains.GetValueOrDefault(instrument.Id);
        if (oc is null)
        {
            _optionChains.Add(instrument.Id, new OptionChain());
            _client.reqSecDefOptParams(instrument.Id, instrument.Symbol, instrument.Exchange, "FUT", instrument.Id);
        }
        else if (!oc.IsOptionChainFresh())
        {
            oc.ClearTradingClasses();
            _client.reqSecDefOptParams(instrument.Id, instrument.Symbol, instrument.Exchange, "FUT", instrument.Id);
        }
        return this;
    }
    public IConnector RequestMarketData(Instrument instrument)
    {
        if (instrument.LastTradeDate < DateTime.Now) return this;
        _callbacks.PriceChange += instrument.OnPriceChange;
        _client.reqMktData(instrument.Id, instrument.ToIbContract(), string.Empty, false, false, null);
        return this;
    }
    public OptionTradingClass? GetOptionTradingClass(int parentId, DateTime approximateDate)
    {
        if (_optionChains.GetValueOrDefault(parentId) is OptionChain chain)
        {
            return chain.GetTradingClass(approximateDate);
        }
        _logger.LogError($"Нет цепочки опционов для инструмента с ID = {parentId}");
        return null;
    }
    #endregion
    #region Transaction/Orders
    public void SendOrder(Instrument instrument, Transaction order)
    {
        order.BrokerId = _callbacks.NextOrderId++;
        _openOrdersCache.Add(order);
        _client.placeOrder(order.BrokerId, instrument.ToIbContract(), order.ToIbOrder());
    }
    #endregion
}
