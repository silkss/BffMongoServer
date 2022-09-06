using ContainerStore.Connectors.Ib.Caches;
using ContainerStore.Connectors.Models;
using IBApi;
using Microsoft.Extensions.Logging;

namespace ContainerStore.Connectors.Ib;

internal class IbCallbacks : DefaultEWrapper
{
	private readonly ILogger<IbConnector> _logger;
	private readonly RequestInstrumentCache _requestInstrument;

	public IbCallbacks(ILogger<IbConnector> logger, RequestInstrumentCache requestInstrument)
	{
		_logger = logger;
		_requestInstrument = requestInstrument;
	}
    public List<Account> Accounts { get; } = new();

    public override void managedAccounts(string accountsList)
	{
        Accounts.Clear();
		foreach (var account in accountsList.Trim().Split(','))
		{
            Accounts.Add(new Account { Name = account });
		}
	}
	public override void error(Exception e)
	{
		_logger.LogCritical(e.Message);
	}
	public override void error(int id, int errorCode, string errorMsg, string advancedOrderRejectJson)
	{
		_logger.LogError($"ERROR_CODE:{errorCode} : MESSAGE:{errorMsg}");
	}
	public override void error(string str)
	{
		_logger.LogError(str);
	}
}
