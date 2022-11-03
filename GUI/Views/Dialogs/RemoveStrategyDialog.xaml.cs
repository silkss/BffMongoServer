using Strategies.DTO;
using System.Windows;

namespace GUI.Views.Dialogs;

/// <summary>
/// Логика взаимодействия для RemoveStrategyDialog.xaml
/// </summary>
public partial class RemoveStrategyDialog : Window
{
    public RemoveStrategyDialog(MainStrategyDTO? strategyDTO)
    {
        InitializeComponent();

        if (strategyDTO == null)
        {
            tbError.Text = "Стратегия равна NULL. ЭТО ПЛОХО!";
            YesButton.IsEnabled = false;
        }
        else if(strategyDTO.Id == null)
        {
            tbError.Text = "У стратегии нет Id!";
            YesButton.IsEnabled = false;
        }
        else
        {
            Strategy = strategyDTO;
        }
    }
    public static readonly DependencyProperty StrategyProperty = DependencyProperty.Register(
        nameof(Strategy),
        typeof(MainStrategyDTO),
        typeof(RemoveStrategyDialog),
        new PropertyMetadata(null));

    public MainStrategyDTO Strategy
    {
        get => (MainStrategyDTO)GetValue(StrategyProperty);
        set => SetValue(StrategyProperty, value);
    }
    private void YesButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    } 
    private void NoButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
