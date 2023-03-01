using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Connectors;
using Connectors.Ib;
using Traders;
using BffLogger;
using Traders.DbSettings.MongoDb;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Logging
    .ClearProviders()
    .AddBffLogger();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();

builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection("DatabaseSettings"));

builder.Services.AddSingleton<ContainerService>();
builder.Services.AddSingleton<ContainerTrader>();
builder.Services.AddSingleton<IConnector, IbConnector>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.MapControllers();


app.Run();
