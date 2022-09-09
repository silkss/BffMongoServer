using ContainerStore.Data.Models;
using ContainerStore.Data.ServiceModel;
using ContainerStore.Gui.Services;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace ContainerStore.Gui.Views;

/// <summary>
/// Логика взаимодействия для ContainersView.xaml
/// </summary>
public partial class ContainersView : Window
{
    private const string PATH = "api/containers";
    public ContainersView()
    {
        InitializeComponent();
    }

    private async void loadContainers(object sender, RoutedEventArgs e)
    {

        var client = AppContext.Client;
        HttpResponseMessage response = await client.GetAsync(PATH);
        if (response.IsSuccessStatusCode)
        {
            var model = await response.Content.ReadAsAsync<List<Container>>();
            model.ForEach(m => dgContainers.Items.Add(m));
        }
    }
}
