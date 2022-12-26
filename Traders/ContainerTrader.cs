using Connectors;
using Strategies;
using MongoDbSettings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Traders;

public class ContainerTrader
{
    private readonly IConnector _connector;
    private readonly ContainerService _containerService;
    private List<Container> _allContainers;
    private List<Container> _containersInTrade = new();

    public ContainerTrader(IConnector connector, ContainerService containerService)
    {
        _connector = connector;
        _containerService = containerService;
        _allContainers = _containerService.Get();
    }

    public IEnumerable<Container> GetAllContainers() => _allContainers;
    public async Task AddContainerAsync(Container container)
    {
        if (_allContainers.Contains(container)) return;
        _allContainers.Add(container);
        await _containerService.CreateAsync(container);
    }
}
