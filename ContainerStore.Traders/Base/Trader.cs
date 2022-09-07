using ContainerStore.Connectors;
using ContainerStore.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace ContainerStore.Traders.Base;

public class Trader
{
    private readonly List<Container> _containers = new();
	private readonly IConnector _connector;

	public Trader(IConnector connector)
	{
		_connector = connector;
	}
	public (bool isAdded, string message) AddToTrade(Container container)
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
		if (container.Straddles == null)
		{
			return (true, "Контейнер добавлен, но у него нет стрэдлов!");
		}
		foreach (var straddle in container.Straddles)
		{
			if (straddle.CallLeg == null || straddle.PutLeg == null) continue;
			_connector.RequestMarketData(straddle.CallLeg.Instrument);
			_connector.RequestMarketData(straddle.PutLeg.Instrument);
		}

		return (true, "Контейнер добавлен!");
	}
	public IEnumerable<Container> GetContainers() => _containers;
	public bool RemoveFromTrade(Container container)
	{
		return _containers.Remove(container);
	}
}
