﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ContainerStore.Gui.Services;

internal static class AppContext
{
    public readonly static HttpClient Client;
	static AppContext()
	{
		Client = new HttpClient();
		Client.BaseAddress = new Uri("http://localhost:5001");
        Client.DefaultRequestHeaders.Accept.Clear();
        Client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }
}
