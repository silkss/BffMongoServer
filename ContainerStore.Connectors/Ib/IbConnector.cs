using ContainerStore.Connectors.Ib.Caches;
using ContainerStore.Connectors.Models;
using IBApi;
using Microsoft.Extensions.Logging;

namespace ContainerStore.Connectors.Ib;

public class IbConnector : IConnector
{
	private readonly RequestInstrumentCache _requestInstrument;
	private readonly IbCallbacks _callbacks;

    private readonly EClientSocket _client;
    private readonly EReaderSignal _signalMonitor = new EReaderMonitorSignal();
    private readonly ILogger<IbConnector> _logger;

    public IbConnector(ILogger<IbConnector> logger)
	{
        _logger = logger;

        _requestInstrument = new();
		_callbacks = new IbCallbacks(_logger, _requestInstrument);
        _client = new EClientSocket(_callbacks, _signalMonitor);
    }
    public string Host { get; private set; } = "127.0.0.1";
    public int Port { get; private set; } = 7497;
    public int ClientId { get; private set; } = 12;
    public bool IsConnected { get => _client.IsConnected(); }
    public void Connect()
    {
        Connect(Host, Port, ClientId);
    }
    public void Connect(string host, int port, int clientId)
    {
        _client.eConnect(host, port, clientId);

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
    public IEnumerable<Account> GetAccounts() => _callbacks.Accounts;
}
