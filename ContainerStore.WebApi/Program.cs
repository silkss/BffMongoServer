using ContainerStore.Connectors;
using ContainerStore.Connectors.Ib;
using ContainerStore.Data.Models;
using ContainerStore.WebApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.Configure<ContainerStoreDatabaseSettings>(
    builder.Configuration.GetSection("ContainerStoreDatabase"));
builder.Services.AddSingleton<ContainersService>();
builder.Services.AddSingleton<IConnector, IbConnector>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();
app.MapControllers();
app.Run();
