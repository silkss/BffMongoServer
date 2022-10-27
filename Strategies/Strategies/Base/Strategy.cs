﻿using System.Diagnostics.Metrics;

namespace Strategies.Strategies.Base;

public class Strategy : ForWpf.PropertyNotifier
{
    private Instrument _instrument;
    public Instrument Instrument
    {
        get => _instrument;
        set => Set(ref _instrument, value);
    }
}
