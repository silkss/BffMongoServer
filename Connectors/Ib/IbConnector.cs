using Connectors.Ib.Caches;
using Connectors.Converters.Ib;
using System.Threading;
using System.Collections.Generic;
using IBApi;
using System;
using Common.Enums;
using System.Linq;
using Common.Helpers;
using Notifier;
using Connectors.Info;
using Instruments.PriceRules;
using Instruments;
using Transactions;

namespace Connectors.Ib;

public class IbConnector : IConnector
{
    private const int CHECK_CONNECTION_INTERVAL = 5; //in minutes
    private readonly RequestInstrumentCache _requestInstrument = new();
    private readonly OpenOrdersCache _openOrdersCache = new();
    private readonly ConnectorInfo _connectionInfo = new();
    private readonly EClientSocket _client;
    private readonly IbCallbacks _callbacks;
    private readonly EReaderSignal _signalMonitor = new EReaderMonitorSignal();
    private readonly IBffLogger _logger;
    private readonly Dictionary<int, List<PriceBorder>> _marketRules = new();
    private readonly Dictionary<int, OptionChain> _optionChains = new();
    private Timer? _timer;
    private Instrument? reqContract(Contract contract)
    {

        var msg = contract.SecType == "FUT" ?
            $"Requested {contract.SecType}: {contract.LocalSymbol}" :
            $"Requested {contract.SecType}: {contract.Strike} {contract.Symbol} {contract.Right}";

        _logger.LogInformation(msg, toTelegram: false);

        var reqid = _callbacks.NextOrderId; 
        _client.reqContractDetails(reqid, contract);

        while (!_requestInstrument.ContainsKey(reqid))
        {
            _requestInstrument.WaitForResponce();
        }
        var responseContract = _requestInstrument.GetByKey(reqid);
        if (responseContract is not null )
        {
            ReqMarketRule(responseContract.MarketRuleId);
        }
        return responseContract;
    }
    private void reqServerTime(object? state)
    {
        _logger.LogInformation($"{DateTime.Now} request time sended.");
        _client.reqCurrentTime();
    }
    private void reconnect(bool isConnected)
    {
        if (isConnected) return;
        if (_connectionInfo.TimeOfLastConnection.AddMinutes(1) < DateTime.Now)
        {
            Connect(_connectionInfo.Host, _connectionInfo.Port, _connectionInfo.ClientId);
        }
    }
    public IbConnector(IBffLogger logger)
	{
        _logger = logger;

		_callbacks = new IbCallbacks(_logger, _requestInstrument, _optionChains, 
            _openOrdersCache, _marketRules, _connectionInfo);
        _callbacks.ConnectionChanged += reconnect;
        _client = new EClientSocket(_callbacks, _signalMonitor);
    }
    #region Connector props
    public ConnectorInfo GetConnectionInfo() => _connectionInfo;
    public IEnumerable<string> GetAccounts() => _connectionInfo.Accounts;
    #endregion
    #region Connect / Disconnect
    public void Connect()
    {
        Connect(_connectionInfo.Host, _connectionInfo.Port, _connectionInfo.ClientId);
    }
    public void Connect(string host, int port, int clientId)
    {
        _connectionInfo.SetSettings(host, port, clientId);
        _connectionInfo.TimeOfLastConnection = DateTime.Now;

        _client.eConnect(_connectionInfo.Host, _connectionInfo.Port, _connectionInfo.ClientId);

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

        _connectionInfo.IsConnected = true;
        _client.reqMarketDataType(3);

        if (_timer == null)
            _timer = new Timer(reqServerTime, null,
                TimeSpan.FromMinutes(CHECK_CONNECTION_INTERVAL),
                TimeSpan.FromMinutes(CHECK_CONNECTION_INTERVAL));
    }
    public void Disconnect()
    {
        _client.eDisconnect();
    }
    public void AddConnectionChangedCallback(Action<bool> callback)
    {
        _callbacks.ConnectionChanged += callback;
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
    public IConnector RequestDependentInstrument(InstrumentType type, OptionType optionType, Instrument parent, 
        double strike, DateTime expDate, out Instrument? instrument)
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
        instrument = reqContract(contract);
        return this;
    }
    public IConnector RequestCall(Instrument parent, double strike, DateTime expirationDate, out Instrument? instrument) =>
        RequestDependentInstrument(InstrumentType.Option, OptionType.Call, parent, strike, expirationDate, out instrument);
    public IConnector RequestPut(Instrument parent, double strike, DateTime expirationDate, out Instrument? instrument) =>
        RequestDependentInstrument(InstrumentType.Option, OptionType.Put, parent, strike, expirationDate, out instrument);
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
    public IConnector RequestMarketData(Instrument? instrument)
    {
        if (instrument == null) return this;
        if (instrument.LastTradeDate < DateTime.Now) return this;
        ReqMarketRule(instrument.MarketRuleId);
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
    public void ReqMarketRule(int id)
    {
        _client.reqMarketRule(id);
    }
    #endregion
    #region Transaction/Orders
    /// <summary>
    /// Посылает Лимитный ордер
    /// </summary>
    /// <param name="instrument"></param>
    /// <param name="order"></param>
    /// <param name="priceShift"> сдвиг цены </param>
    /// <param name="needToRound"> округли цену до минимального тика, если необходимо. </param>
    public void SendLimitOrder(Instrument instrument, Transaction order, int priceShift = 0, bool needToRound = true)
    {
        order.BrokerId = _callbacks.NextOrderId;
        _openOrdersCache.Add(order);
        
        if (needToRound)
        {
            var min_tick = instrument.MinTick;
            try
            {
                if (_marketRules.GetValueOrDefault(instrument.MarketRuleId) is List<PriceBorder> borders)
                {
                    min_tick = borders.OrderByDescending(b => b.LowEdge).First(b => order.LimitPrice > b.Incriment).Incriment;
                }
            }
            catch (InvalidOperationException)
            {
                min_tick = instrument.MinTick;
            }
            if (order.LimitPrice == 0m)
            {
                order.LimitPrice = instrument.TradablePrice(order.Direction);
            }

            order.LimitPrice = Helper.RoundUp(order.LimitPrice, min_tick);

            if (order.Direction == Directions.Buy)
            {
                order.LimitPrice += (min_tick * priceShift);
            }
            else
            {
                order.LimitPrice -= (min_tick * priceShift);
            }
            _client.placeOrder(order.BrokerId, instrument.ToIbContract(), order.ToIbOrder());
        }
        else
        {
            _client.placeOrder(order.BrokerId, instrument.ToIbContract(), order.ToIbOrder());
        }
    }
    public bool IsOrderOpen(Transaction order) => _openOrdersCache.Contains(order);
    public IConnector CancelOrder(Transaction? transaction)
    {
        if (transaction == null) return this;
        _client.cancelOrder(transaction.BrokerId, "");
        return this;
    }
    #endregion
}
