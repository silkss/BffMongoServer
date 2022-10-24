using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ContainerStore.Gui.Converters;

public class PnlColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value switch
    {
        decimal d => d > 0 ? new SolidColorBrush(Colors.ForestGreen) : new SolidColorBrush(Colors.Red),
        _ => new SolidColorBrush(Colors.Blue)
    };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
