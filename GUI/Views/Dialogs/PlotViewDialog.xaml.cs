using OxyPlot.Series;
using OxyPlot;
using System.Windows;
using Strategies.DTO;

namespace GUI.Views.Dialogs;

/// <summary>
/// Логика взаимодействия для PlotView.xaml
/// </summary>
public partial class PlotViewDialog : Window
{
    public PlotViewDialog(MainStrategyDTO strategy)
    {
        // Create the plot model
        var tmp = new PlotModel { Title = "Simple example", Subtitle = "using OxyPlot" };

        // Create two line series (markers are hidden by default)
        var series1 = new LineSeries { Title = "Theor", MarkerType = MarkerType.Circle };
        var base_strike = decimal.ToDouble(strategy.OpenStraddle!.Call!.Instrument!.Strike);
        var theor_profit = decimal.ToDouble(strategy.OpenStraddle!.Call!.OpenPrice);
        theor_profit += decimal.ToDouble(strategy.OpenStraddle!.Put!.OpenPrice);
        series1.Points.Add(new DataPoint(base_strike, 0));
        series1.Points.Add(new DataPoint(10, 18));
        series1.Points.Add(new DataPoint(20, 12));
        series1.Points.Add(new DataPoint(30, 8));
        series1.Points.Add(new DataPoint(40, 15));

        var series2 = new LineSeries { Title = "Series 2", MarkerType = MarkerType.Square };
        series2.Points.Add(new DataPoint(0, 4));
        series2.Points.Add(new DataPoint(10, 12));
        series2.Points.Add(new DataPoint(20, 16));
        series2.Points.Add(new DataPoint(30, 25));
        series2.Points.Add(new DataPoint(40, 5));

        // Add the series to the plot model
        tmp.Series.Add(series1);
        tmp.Series.Add(series2);

        // Axes are created automatically if they are not defined

        // Set the Model property, the INotifyPropertyChanged event will make the WPF Plot control update its content
        this.Model = tmp;

        InitializeComponent();
    }
    public PlotModel Model { get; private set; }
}
