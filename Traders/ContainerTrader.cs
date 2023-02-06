namespace Traders;

using Connectors;
using Strategies;
using MongoDbSettings;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Microsoft.Extensions.Hosting;

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

    public ContainerTrader(IConnector connector, ContainerService containerService, IHostApplicationLifetime lifetime)
    {
        _connector = connector;
        _containerService = containerService;
        _allContainers = _containerService.Get();

        lifetime.ApplicationStopping.Register(StopTrader);

        new Thread(tradingLoop) { IsBackground = true }.Start();
    }

    public IEnumerable<Container> GetAllContainers() => _allContainers;

    public async Task AddContainerAsync(Container container)
    {
        if (_allContainers.Contains(container)) return;
        _allContainers.Add(container);
        await _containerService.CreateAsync(container);
    }
    public void StopTrader()
    {
        foreach(var container in _containersInTrade)
        {
            container.Stop(_connector);
            if (container.Id != null)
                _containerService.UpdateAsync(container.Id, container);
        }
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
    public async Task StopContainerAsync(string containerId)
    {
        Container? stopped = null;
        lock(_containersInTrade)
        {
            stopped = _containersInTrade.FirstOrDefault(c => c.Id == containerId);
            if (stopped != null)
            {
                _containersInTrade.Remove(stopped);
            }
        }
        if (stopped == null) return;
        stopped.Stop(_connector);
        
        if (stopped.Id == null) return;
        await _containerService.UpdateAsync(stopped.Id, stopped);
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
    public Container? GetById(string id) => _allContainers
        .FirstOrDefault(c => c.Id == id);
}
