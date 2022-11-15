using Strategies.DTO;
using System.Windows;

namespace GUI.Views.Dialogs;

/// <summary>
/// Логика взаимодействия для StrategyInfo.xaml
/// </summary>
public partial class StrategyInfo : Window
{
    public StrategyInfo(MainStrategyDTO strategy)
    {
        Strategy = strategy;
        InitializeComponent();
    }

    public static readonly DependencyProperty StrategyProperty = DependencyProperty.Register(
        nameof(Strategy),
        typeof(MainStrategyDTO),
        typeof(StrategyInfo),
        new PropertyMetadata(null));

    public MainStrategyDTO Strategy
    {
        get => (MainStrategyDTO)GetValue(StrategyProperty);
        set => SetValue(StrategyProperty, value);
    }
}
