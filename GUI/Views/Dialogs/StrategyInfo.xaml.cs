using OxyPlot;
using OxyPlot.Series;
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

        if (strategy.OpenStraddle == null) return;

        var straddle = strategy.OpenStraddle;

        var tmp = new PlotModel { Title = "Simple example", Subtitle = "using OxyPlot" };

        var series1 = new LineSeries { Title = "Series 1", MarkerType = MarkerType.Circle };
        series1.Points.Add(new DataPoint(0, 0));
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

        tmp.Series.Add(series1);
        tmp.Series.Add(series2);

        this.Model = tmp;

        //if (strategy.Instrument == null) return;
        //if (straddle.Call == null || straddle.Put == null) return;
        //if (straddle.Call.Instrument == null || straddle.Put.Instrument == null) return;

        //var tmp = new PlotModel { Title = "Simple example", Subtitle = "using OxyPlot" };
        //var mul = Strategy.Instrument.Multiplier;
        //var call_series = new LineSeries { Title = "Open", MarkerType = MarkerType.Circle };
        //var put_series = new LineSeries { Title = "Put", MarkerType = MarkerType.Circle };

        //var call_base = decimal.ToDouble(-straddle.Call.OpenPrice) * mul;
        //var call_end = decimal.ToDouble(straddle.Call.Instrument.Strike * 1.1m - straddle.Call.OpenPrice) * mul;

        //var put_base = decimal.ToDouble(-straddle.Put.OpenPrice) * mul;
        //var put_end = decimal.ToDouble(straddle.Put.Instrument.Strike * 0.1m - straddle.Put.OpenPrice) * mul;

        //call_series.Points.Add(new DataPoint(0, call_base));
        //call_series.Points.Add(new DataPoint(40, call_end));

        //put_series.Points.Add(new DataPoint(0, put_base));
        //put_series.Points.Add(new DataPoint(-40, put_end));

        //// Add the series to the plot model
        //tmp.Series.Add(call_series);
        //tmp.Series.Add(put_series);
        //this.Model = tmp;
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

    public PlotModel Model { get; private set; }
}
