using ContainerStore.Connectors;
using ContainerStore.Connectors.Ib;
using ContainerStore.Traders.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MongoDbSettings;
using TraderBot.Notifier;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

builder.Services.Configure<StrategyDatabaseSettings>(
#if DEBUG
    builder.Configuration.GetSection("DatabaseDev"));
#else
    builder.Configuration.GetSection("StrategiesStoreDb"));
#endif
builder.Services.AddSingleton<StrategyService>();

builder.Services.AddSingleton<IConnector, IbConnector>();
builder.Services.AddSingleton<Trader>();
builder.Services.AddSingleton<Notifier>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.Run();
