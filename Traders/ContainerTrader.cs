namespace Traders;

using Connectors;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Strategies.BatmanStrategy;
using Traders.DbSettings.MongoDb;
using Microsoft.Extensions.Logging;

public class ContainerTrader
{
    private readonly IConnector _connector;
    private readonly ContainerService _containerService;
    private readonly ILogger<ContainerTrader> _logger;
    private List<BatmanContainer> _allContainers;
    private List<BatmanContainer> _containersInTrade = new();
    private bool _isTrading = true;

    private void tradingLoop()
    {
        while (_isTrading)
        {
            lock (_containersInTrade)
            {
                foreach (var container in _containersInTrade)
                    container.Work(_connector, _logger);
            }
            Thread.Sleep(1000);
        }
    }

    public ContainerTrader(
        IConnector connector, 
        ContainerService containerService, 
        IHostApplicationLifetime lifetime,
        ILogger<ContainerTrader> logger)
    {
        _connector = connector;
        _containerService = containerService;
        _logger = logger;
        _allContainers = _containerService.Get();

        lifetime.ApplicationStopping.Register(StopTrader);

        new Thread(tradingLoop) { IsBackground = true }.Start();
    }

    public IEnumerable<BatmanContainer> GetAllContainers() => _allContainers;

    public async Task AddContainerAsync(BatmanContainer container)
    {
        if (_allContainers.Contains(container)) return;
        _allContainers.Add(container);
        await _containerService.CreateAsync(container);
    }
    public void StopTrader()
    {
        foreach(var container in _containersInTrade)
        {
            container.Stop(_connector, _logger);
            if (container.Id != null)
                _containerService.UpdateAsync(container.Id, container).Wait();
        }
        _isTrading = false;
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
                container.Start(_connector, _logger);
            }
        }
    }
    public async Task StopContainerAsync(string containerId)
    {
        BatmanContainer? stopped = null;
        lock(_containersInTrade)
        {
            stopped = _containersInTrade.FirstOrDefault(c => c.Id == containerId);
            if (stopped != null)
            {
                _containersInTrade.Remove(stopped);
            }
        }
        if (stopped == null) return;
        stopped.Stop(_connector, _logger);
        
        if (stopped.Id == null) return;
        await _containerService.UpdateAsync(stopped.Id, stopped);
    }

    public BatmanContainer? GetContainer(string instumentName, string account)
    {
        BatmanContainer? container = null;
        lock (_containersInTrade)
        {
            container = _containersInTrade.FirstOrDefault(c =>
                c.Instrument?.FullName == instumentName.Trim().ToUpper() &&
                c.Settings?.Account == account.Trim().ToUpper());
        }
        return container;
    }
    public BatmanContainer? GetById(string id) => _allContainers
        .FirstOrDefault(c => c.Id == id);
}
