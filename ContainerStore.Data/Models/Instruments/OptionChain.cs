﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ContainerStore.Data.Models.Instruments;

public class OptionChain
{
    private DateTime _requestedTime;
    private readonly TimeSpan _freshPeriod = new TimeSpan(days: 1, hours: 0, minutes: 0, seconds: 0);
    private readonly List<OptionTradingClass> _tradingClasses = new();

    public bool IsOptionChainFresh() => DateTime.UtcNow - _requestedTime < _freshPeriod;
    public void RefreshRequestTime() => _requestedTime = DateTime.UtcNow;
    public void ClearTradingClasses() => _tradingClasses.Clear();
    public void AddTradingClass(OptionTradingClass otc) => _tradingClasses.Add(otc);
    public OptionTradingClass? GetTradingClass(DateTime approximateDate) => _tradingClasses
        .OrderBy(tc => tc.ExpirationDate)
        .FirstOrDefault(tc => tc.ExpirationDate > approximateDate);
}