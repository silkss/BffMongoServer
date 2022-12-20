using Common.Enums;
using Connectors;
using GreatOptionTrader.Commands.Base;
using GreatOptionTraderStrategies.Strategies.Base;
using GreatOptionTraderStrategies.Strategies.Base.Settings;
using Instruments;
using System.Collections.ObjectModel;

namespace GreatOptionTrader.ViewModels;

internal class CreateContainerViewModel : Base.ViewModel
{
	private readonly IConnector _connector;
	public CreateContainerViewModel()
	{
		_connector = Services.Get.Connector;
		Accounts = new(_connector.GetAccounts());

		RequestInstrument = new(onRequestInstrument, canRequestInstrument);
		CreateContainer = new(onCreateContainer, canCreateContainer);
	}

	public ObservableCollection<string> Accounts { get; }

	private string? _account;
	public string? Account
	{
		get => _account;
		set => Set(ref _account, value);
	}

	private int _orderPriceShift;
	public int OrderPriceShift
	{
		get => _orderPriceShift;
		set => Set(ref _orderPriceShift, value);
	}

	private string? _instrumentName;
	public string? InstrumentName
	{
		get => _instrumentName;
		set => Set(ref _instrumentName, value);
	}

	private string? _exchange;
	public string? Exchange
	{
		get => _exchange;
		set => Set(ref _exchange, value);
	}

	private Instrument? _instrument;
	public Instrument? Instrument
	{
		get => _instrument;
		set => Set(ref _instrument, value);
	}

	private Directions _straddleDirection;
	public Directions StraddleDirection
	{
		get => _straddleDirection;
		set => Set(ref _straddleDirection, value);
	}

	private int _minDaysToExpiration;
    public int MinDaysToExpiration
	{
		get => _minDaysToExpiration;
		set => Set(ref _minDaysToExpiration, value);
	}
	private string? _tradingOptionClass;
    public string? TradingOptionClass
	{
		get => _tradingOptionClass; 
		set => Set(ref _tradingOptionClass, value);
	}

    public LambdaCommand RequestInstrument { get; }
	private void onRequestInstrument(object? p)
	{
		if (string.IsNullOrEmpty(InstrumentName) || string.IsNullOrEmpty(Exchange)) return;

		Instrument = _connector.RequestInstrument(InstrumentName, Exchange);
    }
	private bool canRequestInstrument(object? p) => 
		!string.IsNullOrEmpty(InstrumentName) || 
		!string.IsNullOrEmpty(Exchange);

	public LambdaCommand CreateContainer { get; }
	private void onCreateContainer(object? p) 
	{
		if (Instrument == null || 
			string.IsNullOrEmpty(TradingOptionClass) ||
			Account == null) return;

		var container = new Container
		{
			Instrument = Instrument,
			ContainerSettings = new ContainerSettings
			{
				Account = Account,
				OrderShift = OrderPriceShift,
			},
			StraddleSettings = new StraddleSettings
			{
				BaseDirections = StraddleDirection,
				MinDaysToExpiration = MinDaysToExpiration,
				TradingOptionClass = TradingOptionClass
			},
		};
		Services.Get.AddContainer(container);
	}
	private bool canCreateContainer(object? p) =>
		Instrument != null ||
		!string.IsNullOrEmpty(TradingOptionClass) || 
		Account != null;

}
