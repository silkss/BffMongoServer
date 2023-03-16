namespace OptionTraderClient.DI;

using System.Net.Http;
using System;

internal static class Services {
    private static string BuildContainersEndPoint() => "batmans";

    public readonly static HttpClient Client = new() {
#if DEBUG
        BaseAddress = new Uri("http://localhost:5001/api"),
#else
        BaseAddress = new Uri("http://localhost:5000/api"),
#endif
    };
}
