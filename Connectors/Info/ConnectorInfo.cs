using System;
using System.Collections.Generic;

namespace Connectors.Info;

public class ConnectorInfo
{
    public string Host { get;  set; } = "127.0.0.1";
    public int Port { get;  set; } = 7497;
    public int ClientId { get;  set; } = 12;
    public bool IsConnected { get; set; }
    public DateTime TimeOfLastConnection { get; set; }
    public List<string> Accounts { get; } = new();
    public void SetSettings(string host, int port, int clientId)
    {
        Host = host;
        Port = port;
        ClientId = clientId;
    }
}
