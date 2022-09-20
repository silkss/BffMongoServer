using ContainerStore.Data.Models;
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
}
