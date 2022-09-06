using ContainerStore.Connectors.Converters.Ib;
using ContainerStore.Connectors.Ib.Caches;
using ContainerStore.Connectors.Models;
using IBApi;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace ContainerStore.Connectors.Ib;

internal class IbCallbacks : DefaultEWrapper
{
	private readonly ILogger<IbConnector> _logger;
	private readonly RequestInstrumentCache _requestInstrument;

	public int NextOrderId { get; set; }

    public IbCallbacks(ILogger<IbConnector> logger, RequestInstrumentCache requestInstrument)
	{
		_logger = logger;
		_requestInstrument = requestInstrument;
	}
    public List<Account> Accounts { get; } = new();
	public override void contractDetails(int reqId, ContractDetails contractDetails)
	{
		_requestInstrument.Add(reqId, contractDetails.ToInstrument());
	}
	public override void nextValidId(int orderId)
	{
		NextOrderId = orderId;
	}
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
		switch (errorCode)
		{
            case 200: // что-то не то с запросом инструмента.
                _requestInstrument.Add(id, null);
                _requestInstrument.ReceivedSignal();
				_logger.LogError($"Чтото не так с запросом инструмента.CODE:{errorCode}\nMESSAGE:{errorMsg}");
                break;
			default:
                _logger.LogError($"ERROR_CODE:{errorCode} : MESSAGE:{errorMsg}");
				break;
        }
	}
	public override void error(string str)
	{
		_logger.LogError(str);
	}
}
