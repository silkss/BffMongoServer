using Connectors;
using Connectors.Ib;
using Traders.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MongoDbSettings;
using Notifier;

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
builder.Services.AddSingleton<IBffLogger, BaseNotifier>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.Run();
