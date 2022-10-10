using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using ContainerStore.Connectors;
using ContainerStore.Common.Enums;
using ContainerStore.Data.Models;
using ContainerStore.Data.Models.TradeUnits;
using ContainerStore.Data.Models.Instruments;
using ContainerStore.Data.Models.Transactions;
using ContainerStore.WebApi.Services;
using ContainerStore.Traders.Helpers;
using TraderBot.Notifier;

namespace ContainerStore.Traders.Base;

public class Trader
{
	private const int BORDER_MUL = 4; //количество мин тиков для отслеживания смещение цены.

	private readonly object _containersLock = new();
    private readonly List<Container> _containers = new();
	private readonly IConnector _connector;
	private readonly Notifier _logger;
	private readonly ContainersService _containersService;
	private readonly IHostApplicationLifetime _lifetime;
	private bool _strated = true;

	private void onConnectionChanged(bool isConnected)
	{
		if (isConnected)
		{
			lock (_containersLock)
			{
				foreach (var container in _containers)
				{
					_connector
						.RequestMarketData(container.ParentInstrument);
					foreach (var straddle in container.Straddles)
					{
						_connector
							.RequestMarketData(straddle.CallLeg.Instrument)
							.RequestMarketData(straddle.CallLeg.Closure?.Instrument)
							.RequestMarketData(straddle.PutLeg.Instrument)
							.RequestMarketData(straddle.PutLeg.Closure?.Instrument);
					}
				}
			}
		}
		else
		{
			// не знаю пока.
		}
	}
	private void sendOrder(Instrument instrument, Transaction transaction, decimal price, int priceShift = 0)
	{
		_connector.SendOrder(instrument, transaction, price, priceShift);
	}
	private void openClosure(string account, Closure? closure, decimal limitPrice)
    {
        if (closure == null) return;
		if (limitPrice == 0m) return;
		if (closure.Logic == TradeLogic.Close)
		{
			closure.Logic = TradeLogic.Open;
		}
		if (closure.IsDone()) return;
		if (closure.OpenOrder == null)
		{
			sendOrder(closure.Instrument, closure.CreateOrder(account, closure.Direction), limitPrice);
		}
    }
    private void closeClosure(Closure? closure, string account, int orderPriceShift)
    {
		if (closure == null) return;
		
		if (closure.OpenOrder == null)
		{
            if (closure.IsDone()) return;
            sendOrder(
				closure.Instrument, 
				closure.CreateClosingOrder(account), 
				closure.Instrument.TradablePrice(closure.CloseDirection())
				);
			return;
		}
		if (closure.OpenOrder != null)
		{
			if (closure.OpenOrder.Direction != closure.CurrentDirection())
			{
				_connector.CancelOrder(closure.OpenOrder);
				return;
			}
            if (closure.IsDone()) return;
            var up_border = closure.Instrument.TradablePrice(closure.CurrentDirection()) + closure.Instrument.MinTick * BORDER_MUL * orderPriceShift;
            var down_border = closure.Instrument.TradablePrice(closure.CurrentDirection()) - closure.Instrument.MinTick * BORDER_MUL * orderPriceShift;

            if (closure.OpenOrder.LimitPrice > up_border || closure.OpenOrder.LimitPrice < down_border)
            {
                _logger.LogInformation(
					$"order LMT price {closure.OpenOrder.LimitPrice} is out of borders: " +
                    $"up {up_border}, down {down_border}");
                _connector.CancelOrder(closure.OpenOrder);
            }
        }
    }
    private void openStraddleLeg(StraddleLeg leg, string account, int orderPriceShift)
	{
		if (leg.Instrument == null) return;
        if (leg.Instrument.TradablePrice(leg.Direction) == 0)
        {
            _logger.LogError($"{leg.Instrument.FullName}. Tradable price is 0");
			return;
        }

		sendOrder(leg.Instrument, leg.CreateOpeningOrder(account), leg.Instrument.TradablePrice(leg.Direction), orderPriceShift);
	}
	private void closeStraddleLeg(StraddleLeg leg, string account, int orderPriceShift)
	{
		if (leg.Instrument == null) return;
		if (leg.Instrument.TradablePrice(leg.Direction) == 0)
		{
            _logger.LogError($"{leg.Instrument.FullName}. Tradable price is 0");
            return;
        }
        sendOrder(leg.Instrument, 
			leg.CreateClosingOrder(account), 
			leg.Instrument.TradablePrice(leg.CloseDirection()), 
			orderPriceShift);
    }
    private void straddleLegWork(StraddleLeg leg, string account, int orderPriceShift, int closurePriceProcent)
	{
		switch (leg.Logic)
		{
			case TradeLogic.Open when leg.OpenOrder == null:
				if (leg.IsDone())
				{
					var limitPrice = leg.OpenPrice * (closurePriceProcent / 100m);
					openClosure(account, leg.Closure, limitPrice);
				}
				else
				{
                    openStraddleLeg(leg, account, orderPriceShift);
                }
				break;
			case TradeLogic.Close when leg.OpenOrder == null:
				if (!leg.IsDone())
				{
                    closeStraddleLeg(leg, account, orderPriceShift);
                }
				closeClosure(leg.Closure, account, orderPriceShift);
				break;
			case TradeLogic.Close when leg.OpenOrder != null:
			case TradeLogic.Open when leg.OpenOrder != null:
				if (leg.OpenOrder.Direction != leg.CurrentDirection())
				{
					_connector.CancelOrder(leg.OpenOrder);
				}
				else
				{
					var up_border = leg.Instrument.TradablePrice(leg.CurrentDirection()) + leg.Instrument.MinTick * BORDER_MUL * orderPriceShift;
					var down_border = leg.Instrument.TradablePrice(leg.CurrentDirection()) - leg.Instrument.MinTick * BORDER_MUL * orderPriceShift;

					if (leg.OpenOrder.LimitPrice > up_border || leg.OpenOrder.LimitPrice < down_border)
					{
						_logger.LogInformation($"order LMT price {leg.OpenOrder.LimitPrice} is out of borders: up {up_border}, down {down_border}");
						_connector.CancelOrder(leg.OpenOrder);
					}
                }
                closeClosure(leg.Closure, account, orderPriceShift);
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
				straddleLegWork(straddle.CallLeg, container.Account, container.OrderPriceShift, container.ClosurePriceGapProcent);
			if (straddle.PutLeg != null)
				straddleLegWork(straddle.PutLeg, container.Account, container.OrderPriceShift, container.ClosurePriceGapProcent);
		}
	}
	public Trader(IConnector connector, Notifier logger, ContainersService containersService, IHostApplicationLifetime lifetime)
	{
		_connector = connector;
		_logger = logger;
		_containersService = containersService;
		_lifetime = lifetime;

		_connector.AddConnectionChangedCallback(onConnectionChanged);

		_lifetime.ApplicationStopping.Register(() =>
		{
			_logger.LogInformation($"{DateTime.Now}::Останаваливаю торговлю!");
			Stop();
			_containers.ForEach(c =>
			{
				if (c.Id == null) return;
				c.Stop();
				OrdersHelper.CancelContainerOrders(_connector, c);
				_containersService.UpdateAsync(c.Id, c).GetAwaiter();
			});
		});
		Start();
    }
	public void Start()
	{
		if (!_strated) _strated = true;
        new Thread(() =>
        {
            while (_strated)
            {
                lock (_containersLock)
                {
                    _containers.ForEach(c => work(c));
                }
				Thread.Sleep(1000);
            }
        })
        { IsBackground = true }
        .Start();
    }
	public (bool isAdded, string message) AddToTrade(Container container)
	{
		if (!_connector.GetConnectionInfo().IsConnected) { return (false, "Not connected"); }
		lock (_containersLock)
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

			foreach (var straddle in container.Straddles)
			{
				if (straddle.CallLeg is StraddleLeg callLeg)
				{
					_connector.RequestMarketData(callLeg.Instrument);
					if (callLeg.Closure is not null)
					{
						_connector.RequestMarketData(callLeg.Closure.Instrument);
					}
				}
				if (straddle.PutLeg is StraddleLeg putLeg)
				{
					_connector.RequestMarketData(putLeg.Instrument);
                    if (putLeg.Closure is not null)
                    {
                        _connector.RequestMarketData(putLeg.Closure.Instrument);
                    }
                }
			}
			return (true, "Контейнер добавлен!");
		}
	}
	public IEnumerable<Container> GetContainers() => _containers;
	public bool RemoveFromTradeAsync(Container container)
	{
		if (container is null) return false;
		if (container.Id is not null)
		{
            _containersService.UpdateAsync(container.Id, container).GetAwaiter();
        }
		bool res;
		lock (_containersLock)
		{
			res = _containers.Remove(container);
		}
		return res;
	}
    public Container? GetContainer(string symbol, string account)
	{
		Container? container = null;
		lock (_containersLock)
		{
			container = _containers.FirstOrDefault(c => c.ParentInstrument.FullName == symbol && c.Account == account);
		}
		return container;
	}
	public async Task<bool> StopContainerAsync(string id)
	{
		bool removed = false;
		Container? container = null;
		lock (_containersLock)
		{
			container = _containers.FirstOrDefault(c => c.Id == id);
			if (container != null)
			{
				removed = _containers.Remove(container);
			}
		}
		if (container == null) return removed;
		
		foreach (var straddle in container.Straddles)
		{
			OrdersHelper.CancelStraddleOrders(_connector, straddle);
        }
		if (container.Id == null)
		{
			_logger.LogError("Cant save container. NO ID");
			return removed;
		}
        await _containersService.UpdateAsync(container.Id, container);
        return removed;
	}
	public void Stop()
	{
		_strated = false;
	}
}
