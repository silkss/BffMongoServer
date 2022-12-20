using GreatOptionTraderStrategies.Strategies.Base;
using System.Collections.ObjectModel;

namespace GreatOptionTrader.ViewModels;

internal class MainViewModel 
{
    public ObservableCollection<Container> Containers => Services.Get.Containers;
}
