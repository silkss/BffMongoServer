using ContainerStore.Data.Models.Accounts;
using System.Collections.Generic;

namespace ContainerStore.Data.ServiceModel;

public class ConnectorModel
{
    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 7497;
    public int ClientId { get; set; } = 12;
    public bool IsConnected { get; set; }
    public IEnumerable<Account>? Accounts { get; set; }
}
