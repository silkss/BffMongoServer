using ContainerStore.Data.Models;
using ContainerStore.Connectors;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using ContainerStore.Common.Enums;
using ContainerStore.Data.Models.TradeUnits;
using Microsoft.Extensions.Logging;
using ContainerStore.Data.Models.Transactions;
using System.Threading.Tasks;

namespace ContainerStore.Traders.Base;

public class Trader
{
	private readonly object _lock = new();
    private readonly List<Container> _containers = new();
	private readonly IConnector _connector;
	private readonly ILogger<Trader> _logger;
	private bool _strated = true;
	private void sendOrder(Instrument instrument, Transaction transaction)
	{
		_connector.SendOrder(instrument, transaction);
	}
	private void openStraddleLeg(StraddleLeg leg, string account, int orderPriceShift)
	{
		if (!leg.IsDone()) return;
		if (leg.Instrument == null) return;
        if (leg.Instrument.TradablePrice(leg.Direction) == 0)
        {
            _logger.LogError($"{leg.Instrument.FullName}. Tradable price is 0");
			return;
        }

		sendOrder(leg.Instrument, leg.CreateOpeningOrder(account, orderPriceShift));
	}
	private void closeStraddleLeg(StraddleLeg leg, string account, int orderPriceShift)
	{

	}
	private void straddleLegWork(StraddleLeg leg, string account, int orderPriceShift)
	{
		switch (leg.Logic)
		{
			case TradeLogic.Open when leg.OpenOrder == null:
				openStraddleLeg(leg, account, orderPriceShift);
				break;
			case TradeLogic.Close when leg.OpenOrder == null:
				closeStraddleLeg(leg, account, orderPriceShift);
				break;
			default: break;

		}
	}
	private void work(Container container)
	{
		if (container.ParentInstrument == null) return;
		foreach (var straddle in container.Straddles)
		{
			if (straddle.CallLeg != null) 
				straddleLegWork(straddle.CallLeg, container.Account, container.OrderPriceShift);
			if (straddle.PutLeg != null)
				straddleLegWork(straddle.PutLeg, container.Account, container.OrderPriceShift);
		}
	}
	public Trader(IConnector connector, ILogger<Trader> logger)
	{
		_connector = connector;
		_logger = logger;
		Start();
    }
	public void Start()
	{
		if (!_strated) _strated = true;
        new Thread(() =>
        {
            while (_strated)
            {
                lock (_lock)
                {
                    _containers.ForEach(c => work(c));
                }
				Task.Delay(1000);
            }
        })
        { IsBackground = true }
        .Start();
    }
	public (bool isAdded, string message) AddToTrade(Container container)
	{
		lock (_lock)
		{
			if (_containers.FirstOrDefault(c => c.Id == container.Id) is not null)
			{
				return (false, "Контейнер с таким ID уже в торговле!");
			}
			if (container.ParentInstrument == null)
			{
				return (false, "У контейнера не родительского инструмента!");
			}

			_containers.Add(container);
			_connector
				.RequestMarketData(container.ParentInstrument)
				.RequestOptionChain(container.ParentInstrument);
			return (true, "Контейнер добавлен!");
		}
	}
	public IEnumerable<Container> GetContainers() => _containers;
	public bool RemoveFromTrade(Container container)
	{
		return _containers.Remove(container);
	}
    public Container? GetContainer(string symbol, string account)
	{
		Container? container = null;
		lock (_lock)
		{
			container = _containers.FirstOrDefault(c => c.ParentInstrument.FullName == symbol && c.Account == account);
		}
		return container;
	}
	public void Stop()
	{
		_strated = false;
	}
}
