using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using ContainerStore.Connectors;
using ContainerStore.Traders.Helpers;
using TraderBot.Notifier;
using Microsoft.Extensions.Hosting;
using Strategies.Enums;
using Instruments;
using Transactions;
using Strategies.Depend;
using MongoDbSettings;
using Strategies;

namespace ContainerStore.Traders.Base;

public class Trader
{
	private const int BORDER_MUL = 4; //количество мин тиков для отслеживания смещение цены.

	private readonly object _strategyLocker = new();
    private readonly List<MainStrategy> _strategies = new();
	private readonly IConnector _connector;
	private readonly Notifier _logger;
	private readonly StrategyService _strategyService;
    private readonly IHostApplicationLifetime _lifetime;
	private bool _strated = true;

	private void onConnectionChanged(bool isConnected)
	{
		if (isConnected)
		{
			lock (_strategyLocker)
			{
				foreach (var strategy in _strategies)
				{
					strategy.Start(_connector);
				}
			}
		}
		else
		{
			// не знаю пока.
		}
	}

	public Trader(IConnector connector, Notifier logger, StrategyService strategyService, IHostApplicationLifetime lifetime)
	{
		_connector = connector;
		_logger = logger;
        _strategyService = strategyService;
		_lifetime = lifetime;

		_connector.AddConnectionChangedCallback(onConnectionChanged);

		_lifetime.ApplicationStopping.Register(StopTrade);
		Start();
    }
	private void tradingLoop()
	{
		while (_strated)
		{
			lock (_strategyLocker)
			{
				_strategies.ForEach(s => s.Work(_connector, _logger));
			}
			Thread.Sleep(1000);
		}
	}
	public void Start()
	{
		if (!_strated) _strated = true;
		new Thread(tradingLoop)
		{
			IsBackground = true
		}.Start();
    }
	public bool AddToTrade(MainStrategy strategy)
	{
		if (!_connector.GetConnectionInfo().IsConnected) 
		{
			_logger.LogError("Cant Add contrainer. Connector not connected!");
			return false; 
		}
		lock (_strategyLocker)
		{
			if (_strategies.FirstOrDefault(c => c.Id == strategy.Id) is not null)
			{
                _logger.LogWarning("Контейнер с таким ID уже в торговле!");
                return false; 
			}
			if (strategy.Instrument == null)
			{
                _logger.LogError("У контейнера не родительского инструмента!");
                return false;
			}

			_strategies.Add(strategy);
			_connector
				.RequestMarketData(strategy.Instrument)
				.RequestOptionChain(strategy.Instrument);

			foreach (var straddle in strategy.Straddles)
			{
				straddle.Start(_connector);
			}
            _logger.LogInformation("Контейнер добавлен!");
            return true; 
		}
	}
	public IEnumerable<MainStrategy> GetStrategies() => _strategies;
	public bool RemoveFromTradeAsync(MainStrategy strategy)
	{
		if (strategy is null) return false;
		if (strategy.Id is not null)
		{
            _strategyService.UpdateAsync(strategy.Id, strategy).GetAwaiter();
        }
		bool res;
		lock (_strategyLocker)
		{
			res = _strategies.Remove(strategy);
		}
		return res;
	}

    public MainStrategy? GetStrategy(string symbol, string account)
	{
		MainStrategy? strategy = null;
		lock (_strategyLocker)
		{
            strategy = _strategies.FirstOrDefault(c => 
				c.Instrument.FullName == symbol && 
				c.MainSettings?.Account == account);
		}
		return strategy;
	}
	public MainStrategy? GetStrategyById(string id)
	{
		MainStrategy? strategy = null;
		lock(_strategyLocker)
		{
			strategy = _strategies.FirstOrDefault(s => s.Id == id);
		}
		return strategy;
	}
	public async Task<bool> StopContainerAsync(string id)
	{
		bool removed = false;
		MainStrategy? container = null;
		lock (_strategyLocker)
		{
			container = _strategies.FirstOrDefault(c => c.Id == id);
			if (container != null)
			{
				removed = _strategies.Remove(container);
			}
		}
		if (container == null) return removed;

		container.Stop(_connector);

		if (container.Id == null)
		{
			_logger.LogError("Cant save container. NO ID");
			return removed;
		}
        await _strategyService.UpdateAsync(container.Id, container);
        return removed;
	}

	public Task CancelOpenOrderAndSave(MainStrategy strategy)
	{
		strategy.Stop(_connector);
		if (strategy.Id == null)
		{
			return _strategyService.CreateAsync(strategy);
		}
		else
		{
			return _strategyService.UpdateAsync(strategy.Id, strategy);
        }
    }

	public void StopTrade()
	{
        _logger.LogInformation($"{DateTime.Now}::Останаваливаю торговлю!");
        _strated = false;
        _strategies.ForEach(strategy =>
        {
			CancelOpenOrderAndSave(strategy);
        });
		_strategies.Clear();
    }
}
