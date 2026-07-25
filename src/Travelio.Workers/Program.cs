using Travelio.Workers;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Travelio.Infrastructure.Data;
using Travelio.Infrastructure.Services;

var builder = Host.CreateApplicationBuilder(args);
var configuration = builder.Configuration;
builder.Services.AddDbContext<TravelioDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")).UseSnakeCaseNamingConvention());
builder.Services.AddHttpClient("provider1", client => client.BaseAddress = new Uri(configuration["Providers:Provider1:BaseUrl"]!));
builder.Services.AddHttpClient("provider2", client => client.BaseAddress = new Uri(configuration["Providers:Provider2:BaseUrl"]!));
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(configuration["Redis:Configuration"] ?? "localhost:6379,abortConnect=false"));
builder.Services.AddTransient<ProviderAdapter>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
