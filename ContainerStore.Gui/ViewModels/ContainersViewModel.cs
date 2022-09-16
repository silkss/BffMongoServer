using ContainerStore.Data.Models;
using ContainerStore.Data.ServiceModel;
using ContainerStore.Gui.Services;
using ContainerStore.Gui.ViewModels.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows.Documents;

namespace ContainerStore.Gui.ViewModels;

internal class ContainersViewModel : ViewModel
{
    private const string PATH = "api/containers";
    private readonly HttpClient _client;
	public ContainersViewModel()
	{
		_client = AppServices.Client;

		var res = _client.GetAsync(PATH).Result;
		{
            if (!res.IsSuccessStatusCode) return;
            if (res.Content.ReadAsAsync<List<Container>>().Result is List<Container> containers)
            {
                foreach (var container in containers)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Containers.Add(container);
                    });
                }
            }
        }
	}
    public ObservableCollection<Container> Containers { get; } = new();
}
