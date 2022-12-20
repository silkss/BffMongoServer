using Strategies.Strategies;
using System.Collections.ObjectModel;

namespace GreatOptionTrader.ViewModels;

internal class MainViewModel 
{
    public ObservableCollection<MainStrategy> Strategies => Services.Get.Strategies;
}
