using ContainerStore.Connectors;
using ContainerStore.Connectors.Ib;
using ContainerStore.Data.Settings;
using ContainerStore.Traders.Base;
using ContainerStore.WebApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy =>{
        policy.WithOrigins("http://localhost:3000").AllowAnyMethod().AllowAnyHeader();
    });
});
builder.Services.Configure<ContainerStoreDatabaseSettings>(
    builder.Configuration.GetSection("ContainerStoreDatabase"));
builder.Services.AddSingleton<ContainersService>();
builder.Services.AddSingleton<IConnector, IbConnector>();
builder.Services.AddSingleton<Trader>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.Run();
