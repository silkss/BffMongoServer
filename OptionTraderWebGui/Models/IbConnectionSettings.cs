namespace OptionTraderWebGui.Models;

public class IbConnectionSettings
{
    public string Host { get; set; }
    public int Port { get; set; } = 7497;
    public int ClientId { get; set; } = 2;
}
