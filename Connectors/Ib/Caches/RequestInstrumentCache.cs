using Instruments;
using System.Collections.Generic;
using System.Threading;

namespace Connectors.Ib.Caches;

internal class RequestInstrumentCache
{
    private Dictionary<int, Instrument?> _instruments { get; } = new();

    private object locker = new();

    public RequestInstrumentCache Add(int key, Instrument? value)
    {
        _instruments[key] = value;
        ReceivedSignal();
        return this;
    }
    public Instrument? GetByKey(int key)
    {
        var instrument = _instruments.GetValueOrDefault(key);
        _instruments.Remove(key);
        return instrument;
    }

    public bool ContainsKey(int key) => _instruments.ContainsKey(key);
    public void WaitForResponce()
    {
        lock (locker)
        {
            Monitor.Wait(locker);
        }
    }
    public void ReceivedSignal()
    {
        lock (locker)
        {
            Monitor.PulseAll(locker);
        }
    }
}
