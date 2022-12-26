using Connectors;
using Strategies;
using MongoDbSettings;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace Traders;

public class ContainerTrader
{
    private readonly IConnector _connector;
    private readonly ContainerService _containerService;
    private List<Container> _allContainers;
    private List<Container> _containersInTrade = new();

    private void tradingLoop()
    {
        while (true)
        {
            lock (_containersInTrade)
            {
                foreach (var container in _containersInTrade)
                    container.Work(_connector);
            }
            Thread.Sleep(1000);
        }
    }

    public ContainerTrader(IConnector connector, ContainerService containerService)
    {
        _connector = connector;
        _containerService = containerService;
        _allContainers = _containerService.Get();

        new Thread(tradingLoop) { IsBackground = true }.Start();
    }

    public IEnumerable<Container> GetAllContainers() => _allContainers;

    public async Task AddContainerAsync(Container container)
    {
        if (_allContainers.Contains(container)) return;
        _allContainers.Add(container);
        await _containerService.CreateAsync(container);
    }

    public void StartTrade(string containerId)
    {
        var container = _allContainers.FirstOrDefault(c => c.Id == containerId);
        if (container == null) return;

        lock (_containersInTrade)
        {
            if (!_containersInTrade.Contains(container))
            {
                _containersInTrade.Add(container);
                container.Start(_connector);
            }
        }
    }

    public Container? GetContainer(string instumentName, string account)
    {
        Container? container = null;
        lock (_containersInTrade)
        {
            container = _containersInTrade.FirstOrDefault(c =>
                c.Instrument.FullName == instumentName.Trim().ToUpper() &&
                c.ContainerSettings.Account == account.Trim().ToUpper());
        }
        return container;
    }
}
