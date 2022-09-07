using ContainerStore.Connectors.Converters.Ib;
using ContainerStore.Connectors.Ib.Caches;
using ContainerStore.Data.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using IBApi;
using ContainerStore.Data.Models.Instruments;
using ContainerStore.Data.Models.Accounts;

namespace ContainerStore.Connectors.Ib;

public class IbConnector : IConnector
{
	private readonly RequestInstrumentCache _requestInstrument;
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

        _requestInstrument = new();
		_callbacks = new IbCallbacks(_logger, _requestInstrument, _optionChains);
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
    public void RequestOptionChain(Instrument instrument)
    {
        if (instrument.Type != Common.Enums.InstrumentType.Future) return;
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
    }
    public void RequestMarketData(Instrument instrument)
    {
        _callbacks.PriceChange += instrument.OnPriceChange;
        _client.reqMktData(instrument.Id, instrument.ToIbContract(), string.Empty, false, false, null);
    }
}
