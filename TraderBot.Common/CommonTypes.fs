namespace TraderBot.Common

open System

module CommonTypes =
    type Exchange = | Nymex | Globex 
    type Currency = | USD

    type Future = {
        Id: int
        Name: string
        Symbol: string
        Exchange: Exchange
        Currency: Currency
        LastTradeDate: DateTime
    }

    type Option = {
        Id: int
        Name: string
        Symbol: string
        TradingClass: string
        Exchange: Exchange
        Currency: Currency
        Strike: decimal
        LastTradeDate: DateTime
    }

    type Instrument = 
        | Future of Future
        | Option of Option

    let getIbExchange exchange : string =
        match exchange with
        | Nymex -> "NYMEX"
        | Globex -> "GLOBEX"

    let getIbCurrency currency : string=
        match currency with
        | USD -> "USD"

    let createIbFuture (future: Future): IBApi.Contract = 
        let ibf= new IBApi.Contract()
        ibf.ConId <- future.Id
        ibf.LastTradeDateOrContractMonth <- future.LastTradeDate.ToString("yyyyMMdd")
        ibf.LocalSymbol <- future.Symbol
        ibf.LocalSymbol <- future.Name
        ibf.SecType <- "FUT"
        ibf.Currency <- getIbCurrency future.Currency
        ibf.Exchange <- getIbExchange future.Exchange
        ibf

    let createIbOption (option: Option): IBApi.Contract =
        let ibo = new IBApi.Contract()
        ibo.ConId <- option.Id
        ibo.LastTradeDateOrContractMonth <- option.LastTradeDate.ToString("yyyyMMdd")
        ibo.TradingClass <- option.TradingClass
        ibo.LocalSymbol <- option.Symbol
        ibo.LocalSymbol <- option.Name
        ibo.Strike <- Convert.ToDouble(option.Strike)
        ibo.SecType <- "FOP"
        ibo.Currency <- getIbCurrency option.Currency
        ibo.Exchange <- getIbExchange option.Exchange
        ibo

    let instrumentToIbContract (instrument: Instrument) : IBApi.Contract =
        match instrument with
        | Option o -> createIbOption o 
        | Future f -> createIbFuture f 