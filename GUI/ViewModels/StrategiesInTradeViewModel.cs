using GUI.ViewModels.Base;
using Strategies.DTO;
using System.Collections.ObjectModel;

namespace GUI.ViewModels;

internal class StrategiesInTradeViewModel : ViewModel
{

	public StrategiesInTradeViewModel()
	{
		Services.Get.TradeRequests.RefreshAsync();
	}
    public ObservableCollection<MainStrategyDTO> Strategies => Services.Get.StrategiesInTrade;

	private MainStrategyDTO? _selectedStrategy;
	public MainStrategyDTO? SelectedStrategy
	{
		get => _selectedStrategy;
		set => Set(ref _selectedStrategy, value);
	}
}
