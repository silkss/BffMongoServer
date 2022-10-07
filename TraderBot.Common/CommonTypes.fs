namespace TraderBot.Common

module CommonTypes =
    type InstrumentType = |Future |Option
    type Exchange = |Nymex |Globex

    type Instrument = {
        Name: string
        Symbol: string
        Type: InstrumentType
        Exchange: Exchange
    }
