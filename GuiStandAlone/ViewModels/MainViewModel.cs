using GuiStandAlone.Commands.Base;
using Strategies;
using System.Collections.ObjectModel;

namespace GuiStandAlone.ViewModels;

internal class MainViewModel
{
	public MainViewModel()
	{
        LoadStrategies = new LambdaCommand(onLoadStrategiesAsync);
	}

	public ObservableCollection<MainStrategy> Strategies { get; } = new();

	public LambdaCommand LoadStrategies { get; } 
	private async void onLoadStrategiesAsync(object? p)
	{
		var strategies = await Services.Get.StrategyService.GetAsync();
		App.Current.Dispatcher.Invoke(() =>
		{
			foreach (var strat in strategies)
				Strategies.Add(strat);
		});
    }
}
