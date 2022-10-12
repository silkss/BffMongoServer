using ContainerStore.Common.DTO;
using ContainerStore.Data.Models;
using ContainerStore.Gui.Services;
using System;
using System.Net.Http;
using System.Windows;

namespace ContainerStore.Gui.Views;

/// <summary>
/// Логика взаимодействия для ContainerView.xaml
/// </summary>
public partial class ContainerView : Window
{
    public ContainerView()
    {
        InitializeComponent();
    }
    public bool IsContainerInTrade { get; set; }

    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
        nameof(Message),
        typeof(string),
        typeof(ContainerView),
        new PropertyMetadata(null));

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public static readonly DependencyProperty ContainerProperty =
        DependencyProperty.Register(
            nameof(Container),
            typeof(Container),
            typeof(ContainerView),
            new PropertyMetadata(null));

    public Container Container
    {
        get => (Container)GetValue(ContainerProperty);
        set => SetValue(ContainerProperty, value);
    }

    private async void UpdateStraddle(object sender, RoutedEventArgs e)
    {
        var client = AppServices.Client;
        var endpoint = AppServices.CONTAINERS_ENDPOINT;

        var dto = new ContainerDTO
        {
            Account = Container.Account,
            ClosurePriceGapProcent = Container.ClosurePriceGapProcent,
            ClosureStrikeStep = Container.ClosureStrikeStep,
            OrderPriceShift = Container.OrderPriceShift,
            StraddleExpirationDays = Container.StraddleExpirationDays,
            StraddleLiveDays = Container.StraddleLiveDays
        };

        if (Container.Id != null)
        {
            try
            {
                var res = await client.PutAsJsonAsync(endpoint + Container.Id, dto);
                if (res.IsSuccessStatusCode)
                {
                    Message = "Updated";
                }
                else
                {
                    Message = $"Error: {res.StatusCode}";
                }
            }
            catch (Exception exp)
            {
                Message = exp.Message;
            }
        }   
    }
}
