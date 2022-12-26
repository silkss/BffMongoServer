using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Connectors;
using Connectors.Ib;
using Traders;
using Notifier;
using MongoDbSettings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();

builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection("DatabaseSettingsDev"));

builder.Services.AddSingleton<ContainerService>();
builder.Services.AddSingleton<ContainerTrader>();
builder.Services.AddSingleton<IConnector, IbConnector>();
builder.Services.AddSingleton<IBffLogger, BaseNotifier>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseEndpoints(endpoints => {
    endpoints.MapRazorPages();
    endpoints.MapControllers();
});

app.Run();
