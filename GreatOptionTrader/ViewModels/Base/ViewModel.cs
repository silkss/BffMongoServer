using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GreatOptionTrader.ViewModels.Base;

internal class ViewModel : INotifyPropertyChanged
{

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (propertyName == null)
        {
            throw new System.ArgumentNullException(nameof(propertyName));
        }
        if (Equals(field, value)) return false;
        field = value;
        NotifyPropertyChanged(propertyName);
        return true;
    }
}
