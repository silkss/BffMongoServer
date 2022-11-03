using Strategies;
using Strategies.DTO;
using System.Collections.ObjectModel;

namespace GUI.ViewModels;

internal class AllContainersViewModel : Base.ViewModel
{
	public AllContainersViewModel()
	{
		Services.Get.RequestAllStrategies();
	}
	public ObservableCollection<MainStrategyDTO> Strategies => Services.Get.AllSatrategies;

	private MainStrategyDTO? _selecteStrategy;
	public MainStrategyDTO? SelectedStrategy
	{
		get => _selecteStrategy;
		set => Set(ref _selecteStrategy, value);
	}
}
