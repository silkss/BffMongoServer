using Connectors;
using GreatOptionTrader.Commands.Base;
using Instruments;
using System.Collections.ObjectModel;

namespace GreatOptionTrader.ViewModels;

internal class CreateStrategyViewModel : Base.ViewModel
{
	private readonly IConnector _connector;
	public CreateStrategyViewModel()
	{
		_connector = Services.Get.Connector;
		Accounts = new(_connector.GetAccounts());

		RequestInstrument = new(onRequestInstrument, canRequestInstrument);
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

	public LambdaCommand RequestInstrument { get; }
	private void onRequestInstrument(object? p)
	{
		if (string.IsNullOrEmpty(InstrumentName) || string.IsNullOrEmpty(Exchange)) return;

		Instrument = _connector.RequestInstrument(InstrumentName, Exchange);
			 
    }
	private bool canRequestInstrument(object? p) => 
		!string.IsNullOrEmpty(InstrumentName) || 
		!string.IsNullOrEmpty(Exchange);
}
