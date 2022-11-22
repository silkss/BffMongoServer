using ContainerStore.Data.Models.Accounts;
using System;
using System.Collections.Generic;

namespace ContainerStore.Data.ServiceModel.OLD;

public class ConnectorModel
{
    public string Host { get;  set; } = "127.0.0.1";
    public int Port { get;  set; } = 7497;
    public int ClientId { get;  set; } = 12;
    public bool IsConnected { get; set; }
    public DateTime TimeOfLastConnection { get; set; }
    public List<Account> Accounts { get; } = new();
    public void SetSettings(string host, int port, int clientId)
    {
        Host = host;
        Port = port;
        ClientId = clientId;
    }
}
