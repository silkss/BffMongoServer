using ContainerStore.Data.Models;
using ContainerStore.Connectors;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace ContainerStore.Traders.Base;

public class Trader
{
	private readonly object _lock = new();
    private readonly List<Container> _containers = new();
	private readonly IConnector _connector;
	private void work(Container container)
	{
		if (container.ParentInstrument == null) return;
		container.TotalPnl = container.ParentInstrument.Last;
	}
	public Trader(IConnector connector)
	{
		_connector = connector;

		new Thread(() =>
		{
			while (true)
			{
				lock (_lock)
				{
					_containers.ForEach(c => work(c));
				}
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
			_connector.RequestMarketData(container.ParentInstrument);
			return (true, "Контейнер добавлен!");
		}
	}
	public IEnumerable<Container> GetContainers() => _containers;
	public bool RemoveFromTrade(Container container)
	{
		return _containers.Remove(container);
	}

}
